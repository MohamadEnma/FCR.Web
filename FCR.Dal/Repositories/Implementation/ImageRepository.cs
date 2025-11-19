using FCR.Dal.Classes;
using FCR.Dal.Data;
using FCR.Dal.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace FCR.Dal.Repositories.Implementation
{
    public class ImageRepository : GenericRepository<Image>, IImageRepository  // CHANGED: public and inherit from GenericRepository
    {
        private readonly ApplicationDbContext _db;

        public ImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Image>> GetImagesByCarIdAsync(int carId, CancellationToken cancellationToken = default)
        {
            return await _db.Images
                .Where(i => i.CarId == carId)
                .OrderBy(i => i.DisplayOrder)
                .ThenByDescending(i => i.IsPrimary)
                .ToListAsync(cancellationToken);
        }

        public async Task<Image?> GetPrimaryImageAsync(int carId, CancellationToken cancellationToken = default)
        {
            return await _db.Images
                .Where(i => i.CarId == carId && i.IsPrimary)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> SetPrimaryImageAsync(int imageId, int carId, CancellationToken cancellationToken = default)
        {
            // Remove primary flag from all images of this car
            var allImages = await _db.Images
                .Where(i => i.CarId == carId)
                .ToListAsync(cancellationToken);

            foreach (var img in allImages)
            {
                img.IsPrimary = img.ImageId == imageId;
            }

            return await _db.SaveChangesAsync(cancellationToken) > 0;
        }

        public async Task DeleteImagesByCarIdAsync(int carId, CancellationToken cancellationToken = default)
        {
            var images = await _db.Images
                .Where(i => i.CarId == carId)
                .ToListAsync(cancellationToken);

            _db.Images.RemoveRange(images);
        }
    }
}