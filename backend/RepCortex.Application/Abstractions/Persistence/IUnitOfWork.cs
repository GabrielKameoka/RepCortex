namespace RepCortex.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<ITransaction> BeginTransactionAsync();
}

public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync();
    Task RollbackAsync();
}
