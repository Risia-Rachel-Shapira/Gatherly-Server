using Gatherly.Data.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gatherly.Tests.Concurrency
{
    public class ItemConcurrencyTests
    {
        [Fact]
        public async Task TwoContexts_UpdatingSameItem_SecondSaveThrowsConcurrencyException()
        {

            var connectionString =
    "Host=localhost;Port=5432;Database=GatherlyDb;Username=postgres;Password=RACHELI97341";

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