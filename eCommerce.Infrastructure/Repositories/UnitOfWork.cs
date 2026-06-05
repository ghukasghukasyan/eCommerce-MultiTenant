using eCommerce.Application.Services.Interfaces;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories
{
    public class UnitOfWork(ECommerceContext context) : IUnitOfWork
    {
        public async Task ExecuteInTransactionAsync(Func<Task> operation)
        {
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await context.Database.BeginTransactionAsync();
                try
                {
                    await operation();
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
