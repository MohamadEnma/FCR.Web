using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Dal.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);  
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);  
        Task AddAsync(T entity, CancellationToken cancellationToken = default);  
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);  
        Task DeleteAsync(int id, CancellationToken cancellationToken = default); 
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);  
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);  
    }
}