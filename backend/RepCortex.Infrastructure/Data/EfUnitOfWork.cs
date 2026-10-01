using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using RepCortex.Application.Abstractions.Persistence;

namespace RepCortex.Infrastructure.Data;

public sealed class EfUnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task<ITransaction> BeginTransactionAsync() =>
        new EfTransaction(await context.Database.BeginTransactionAsync());

    private sealed class EfTransaction(IDbContextTransaction transaction) : ITransaction
    {
        public Task CommitAsync() => transaction.CommitAsync();
        public Task RollbackAsync() => transaction.RollbackAsync();
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
