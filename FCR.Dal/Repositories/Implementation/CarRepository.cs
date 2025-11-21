using FCR.Dal.Classes;
using FCR.Dal.Data;
using FCR.Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Dal.Repositories.Implementation
{
    public class CarRepository : GenericRepository<Car>, ICarRepository
    {
        private readonly ApplicationDbContext _db;

        public CarRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Car?> GetCarWithImagesAsync(int carId, CancellationToken cancellationToken = default)
        {
            return await _db.Cars
                .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(c => c.CarId == carId && !c.IsDeleted, cancellationToken);
        }

        public async Task<Car?> GetCarWithBookingsAsync(int carId, CancellationToken cancellationToken = default)
        {
            return await _db.Cars
                .Include(c => c.Bookings)
                .FirstOrDefaultAsync(c => c.CarId == carId && !c.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Car>> GetAllWithImagesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Cars
                .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
                .Include(c => c.Bookings)
                .Where(c => !c.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Car>> GetAvailableCarsAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Cars
                .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
                .Where(c => c.IsAvailable && !c.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Car>> GetCarsByBrandAsync(string brand, CancellationToken cancellationToken = default)
        {
            return await _db.Cars
                .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
                .Where(c => c.Brand.ToLower() == brand.ToLower() && !c.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Car>> GetCarsByCategoryAsync(string category, CancellationToken cancellationToken = default)
        {
            return await _db.Cars
                .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
                .Where(c => c.Category.ToLower() == category.ToLower() && !c.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Car>> SearchCarsAsync(string keyword, CancellationToken cancellationToken = default)
        {
            var lowerKeyword = keyword.ToLower();

            return await _db.Cars
                .Include(c => c.Images.OrderBy(i => i.DisplayOrder))
                .Where(c => !c.IsDeleted &&
                    (c.Brand.ToLower().Contains(lowerKeyword) ||
                     c.ModelName.ToLower().Contains(lowerKeyword) ||
                     c.Category.ToLower().Contains(lowerKeyword) ||
                     (c.Description != null && c.Description.ToLower().Contains(lowerKeyword))))
                .ToListAsync(cancellationToken);
        }
        
    }
}