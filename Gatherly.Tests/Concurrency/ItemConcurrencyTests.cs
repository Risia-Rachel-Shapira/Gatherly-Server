using Gatherly.Core.Models;
using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gatherly.Tests.Concurrency
{
    /// <summary>
    /// A [Fact] that is skipped (not failed) when the GATHERLY_TEST_DB
    /// environment variable is not set.
    /// </summary>
    public sealed class RequiresTestDbFactAttribute : FactAttribute
    {
        public const string EnvVarName = "GATHERLY_TEST_DB";

        public RequiresTestDbFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvVarName)))
            {
                Skip = $"Skipped: environment variable '{EnvVarName}' is not set. " +
                       "Set it to a PostgreSQL connection string to run this integration test.";
            }
        }
    }

    public class ItemConcurrencyTests
    {
        [RequiresTestDbFact]
        public async Task TwoContexts_UpdatingSameItem_SecondSaveThrowsConcurrencyException()
        {
            var connectionString =
                Environment.GetEnvironmentVariable(RequiresTestDbFactAttribute.EnvVarName)!;

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            var runId = Guid.NewGuid().ToString("N");
            int? tempItemId = null;

            try
            {
                // Create a temporary item of our own. An existing event and its
                // organizer are only referenced (read-only) to satisfy the FKs.
                await using (var setupContext = new AppDbContext(options))
                {
                    var owner = await setupContext.Events
                        .AsNoTracking()
                        .OrderBy(e => e.Id)
                        .Select(e => new { e.Id, e.OrganizerId })
                        .FirstOrDefaultAsync();

                    Assert.True(owner is not null,
                        "The test database must contain at least one event.");

                    var tempItem = new Item
                    {
                        EventId = owner!.Id,
                        AddedByUserId = owner.OrganizerId,
                        Name = $"concurrency-test-{runId}",
                        Description = "Temporary item created by ItemConcurrencyTests",
                        Quantity = 1
                    };

                    setupContext.Items.Add(tempItem);
                    await setupContext.SaveChangesAsync();
                    tempItemId = tempItem.Id;
                }

                await using var context1 = new AppDbContext(options);
                await using var context2 = new AppDbContext(options);

                var item1 = await context1.Items.FindAsync(tempItemId.Value);
                var item2 = await context2.Items.FindAsync(tempItemId.Value);

                Assert.NotNull(item1);
                Assert.NotNull(item2);

                item1!.Description = $"Updated by context 1 {runId}";
                item2!.Description = $"Updated by context 2 {runId}";

                await context1.SaveChangesAsync();

                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    async () => await context2.SaveChangesAsync());
            }
            finally
            {
                if (tempItemId is int id)
                {
                    await using var cleanupContext = new AppDbContext(options);
                    await cleanupContext.Items
                        .Where(i => i.Id == id)
                        .ExecuteDeleteAsync();
                }
            }
        }
    }
}
