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

            int itemId;

            await using (var setupContext = new AppDbContext(options))
            {
                itemId = await setupContext.Items
                    .Select(i => i.Id)
                    .FirstAsync();
            }

            await using var context1 = new AppDbContext(options);
            await using var context2 = new AppDbContext(options);

            var item1 = await context1.Items.FindAsync(itemId);
            var item2 = await context2.Items.FindAsync(itemId);

            Assert.NotNull(item1);
            Assert.NotNull(item2);

            var runId = Guid.NewGuid().ToString("N");

            item1!.Description = $"Updated by context 1 {runId}";
            item2!.Description = $"Updated by context 2 {runId}";

            await context1.SaveChangesAsync();

            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                async () => await context2.SaveChangesAsync());
        }
    }
}
