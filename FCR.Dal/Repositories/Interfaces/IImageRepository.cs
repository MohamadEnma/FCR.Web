using FCR.Dal.Classes;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Dal.Repositories.Interfaces
{
    public interface IImageRepository : IGenericRepository<Image>  
    {
        Task<IEnumerable<Image>> GetImagesByCarIdAsync(int carId, CancellationToken cancellationToken = default);
        Task<Image?> GetPrimaryImageAsync(int carId, CancellationToken cancellationToken = default);
        Task<bool> SetPrimaryImageAsync(int imageId, int carId, CancellationToken cancellationToken = default);
        Task DeleteImagesByCarIdAsync(int carId, CancellationToken cancellationToken = default);
    }
}