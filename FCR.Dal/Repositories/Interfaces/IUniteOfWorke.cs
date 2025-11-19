using System;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Dal.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable  
    {
   
        ICarRepository Cars { get; }
        IBookingRepository Bookings { get; }
        IImageRepository Images { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}