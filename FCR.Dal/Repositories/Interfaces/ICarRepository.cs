using FCR.Dal.Classes;


namespace FCR.Dal.Repositories.Interfaces
{
    public interface ICarRepository : IGenericRepository<Car>
    {
        // Get cars with related data
        Task<Car?> GetCarWithImagesAsync(int carId, CancellationToken cancellationToken = default);
        Task<Car?> GetCarWithBookingsAsync(int carId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Car>> GetAllWithImagesAsync(CancellationToken cancellationToken = default);

        // Filter and search
        Task<IEnumerable<Car>> GetAvailableCarsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Car>> GetCarsByBrandAsync(string brand, CancellationToken cancellationToken = default);
        Task<IEnumerable<Car>> GetCarsByCategoryAsync(string category, CancellationToken cancellationToken = default);
        Task<IEnumerable<Car>> SearchCarsAsync(string keyword, CancellationToken cancellationToken = default);
       
    }
}