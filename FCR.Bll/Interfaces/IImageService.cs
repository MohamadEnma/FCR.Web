using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Image;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Interfaces
{
    public interface IImageService
    {
        // Upload Images
        Task<ServiceResponse<ImageResponseDto>> UploadImageAsync(
            int carId,
            IFormFile imageFile,
            string? altText,
            bool isPrimary,
            int displayOrder,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<List<ImageResponseDto>>> UploadMultipleImagesAsync(
            int carId,
            List<IFormFile> imageFiles,
            CancellationToken cancellationToken = default);

        // Get Images
        Task<ServiceResponse<ImageResponseDto>> GetImageByIdAsync(
            int imageId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<ImageResponseDto>>> GetImagesByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<ImageResponseDto>> GetPrimaryImageAsync(
            int carId,
            CancellationToken cancellationToken = default);

        // Update Images
        Task<ServiceResponse<ImageResponseDto>> UpdateImageAsync(
            int imageId,
            string? altText,
            int? displayOrder,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> SetPrimaryImageAsync(
            int imageId,
            int carId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> ReorderImagesAsync(
            int carId,
            Dictionary<int, int> imageIdToDisplayOrder,
            CancellationToken cancellationToken = default);

        // Delete Images
        Task<ServiceResponse<bool>> DeleteImageAsync(
            int imageId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> DeleteImagesByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default);

        // Validation
        Task<ServiceResponse<bool>> ValidateImageFileAsync(
            IFormFile file,
            CancellationToken cancellationToken = default);



    }
}