namespace eCommerce.Domain.Interfaces
{
    public interface IGeneric<TEntitty> where TEntitty : class
    {
        Task<IEnumerable<TEntitty>> GetAllAsync();
        Task<TEntitty> GetByIdAsync(Guid id);
        Task<int> AddAsync(TEntitty tentitty);
        Task<int> UpdateAsync(TEntitty tentitty);
        Task<int> DeleteAsync(Guid id);
    }
}
