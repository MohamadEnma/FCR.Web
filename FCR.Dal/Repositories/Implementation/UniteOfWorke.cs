using FCR.Dal.Data;
using FCR.Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Dal.Repositories.Implementation
{
    public class UnitOfWork : IUnitOfWork  
    {
        private readonly ApplicationDbContext _db;
        private IDbContextTransaction? _transaction;

        // Lazy initialization of repositories
        private ICarRepository? _cars;
        private IBookingRepository? _bookings;
        private IImageRepository? _images;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }

       
        public ICarRepository Cars => _cars ??= new CarRepository(_db);
        public IBookingRepository Bookings => _bookings ??= new BookingRepository(_db);
        public IImageRepository Images => _images ??= new ImageRepository(_db);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _db?.Dispose();
        }
    }
}