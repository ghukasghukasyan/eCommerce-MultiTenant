namespace eCommerce.Application.Services.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task ExecuteInTransactionAsync(Func<Task> operation);
    }
}
