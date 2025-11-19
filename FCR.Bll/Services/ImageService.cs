using FCR.Bll.Common;
using FCR.Bll.DTOs.Image;
using FCR.Bll.Interfaces;
using FCR.Dal.Classes;
using FCR.Dal.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Services
{
    public class ImageService : IImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public ImageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<ImageResponseDto>> UploadImageAsync(
            int carId,
            IFormFile imageFile,
            string? altText,
            bool isPrimary,
            int displayOrder,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate file
                var validation = await ValidateImageFileAsync(imageFile, cancellationToken);
                if (!validation.Success)
                {
                    return ServiceResponse<ImageResponseDto>.ErrorResponse(
                        "Invalid image file",
                        validation.Errors?.FirstOrDefault() ?? "Validation failed");
                }

                // Check if car exists
                var car = await _unitOfWork.Cars.GetByIdAsync(carId, cancellationToken);
                if (car == null || car.IsDeleted)
                {
                    return ServiceResponse<ImageResponseDto>.ErrorResponse(
                        "Car not found",
                        "Invalid car ID");
                }
                
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var imageUrl = await SaveImageFileAsync(imageFile, fileName, cancellationToken);

                if (isPrimary)
                {
                    await _unitOfWork.Images.SetPrimaryImageAsync(0, carId, cancellationToken);
                }

                // Create image record
                var image = new Image
                {
                    CarId = carId,
                    Url = imageUrl,
                    AltText = altText ?? $"{car.Brand} {car.Model}",
                    IsPrimary = isPrimary,
                    DisplayOrder = displayOrder,
                    UploadedAt = DateTime.UtcNow
                };

                await _unitOfWork.Images.AddAsync(image, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = new ImageResponseDto
                {
                    ImageId = image.ImageId,
                    Url = image.Url,
                    AltText = image.AltText,
                    IsPrimary = image.IsPrimary,
                    DisplayOrder = image.DisplayOrder
                };

                return ServiceResponse<ImageResponseDto>.SuccessResponse(
                    response,
                    "Image uploaded successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<ImageResponseDto>.ErrorResponse(
                    "Failed to upload image",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<List<ImageResponseDto>>> UploadMultipleImagesAsync(
            int carId,
            List<IFormFile> imageFiles,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var responses = new List<ImageResponseDto>();
                var displayOrder = 1;

                foreach (var file in imageFiles)
                {
                    var result = await UploadImageAsync(
                        carId,
                        file,
                        null,
                        displayOrder == 1, // First image is primary
                        displayOrder,
                        cancellationToken);

                    if (result.Success && result.Data != null)
                    {
                        responses.Add(result.Data);
                    }

                    displayOrder++;
                }

                return ServiceResponse<List<ImageResponseDto>>.SuccessResponse(
                    responses,
                    $"{responses.Count} images uploaded successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<ImageResponseDto>>.ErrorResponse(
                    "Failed to upload images",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<ImageResponseDto>> GetImageByIdAsync(
            int imageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var image = await _unitOfWork.Images.GetByIdAsync(imageId, cancellationToken);
                if (image == null)
                {
                    return ServiceResponse<ImageResponseDto>.ErrorResponse(
                        "Image not found",
                        "Invalid image ID");
                }

                var response = new ImageResponseDto
                {
                    ImageId = image.ImageId,
                    Url = image.Url,
                    AltText = image.AltText,
                    IsPrimary = image.IsPrimary,
                    DisplayOrder = image.DisplayOrder
                };

                return ServiceResponse<ImageResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<ImageResponseDto>.ErrorResponse(
                    "Failed to retrieve image",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<ImageResponseDto>>> GetImagesByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var images = await _unitOfWork.Images.GetImagesByCarIdAsync(carId, cancellationToken);

                var response = images.Select(i => new ImageResponseDto
                {
                    ImageId = i.ImageId,
                    Url = i.Url,
                    AltText = i.AltText,
                    IsPrimary = i.IsPrimary,
                    DisplayOrder = i.DisplayOrder
                }).ToList();

                return ServiceResponse<IEnumerable<ImageResponseDto>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<ImageResponseDto>>.ErrorResponse(
                    "Failed to retrieve images",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<ImageResponseDto>> GetPrimaryImageAsync(
            int carId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var image = await _unitOfWork.Images.GetPrimaryImageAsync(carId, cancellationToken);
                if (image == null)
                {
                    return ServiceResponse<ImageResponseDto>.ErrorResponse(
                        "No primary image found",
                        "This car has no primary image");
                }

                var response = new ImageResponseDto
                {
                    ImageId = image.ImageId,
                    Url = image.Url,
                    AltText = image.AltText,
                    IsPrimary = image.IsPrimary,
                    DisplayOrder = image.DisplayOrder
                };

                return ServiceResponse<ImageResponseDto>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ServiceResponse<ImageResponseDto>.ErrorResponse(
                    "Failed to retrieve primary image",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<ImageResponseDto>> UpdateImageAsync(
            int imageId,
            string? altText,
            int? displayOrder,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var image = await _unitOfWork.Images.GetByIdAsync(imageId, cancellationToken);
                if (image == null)
                {
                    return ServiceResponse<ImageResponseDto>.ErrorResponse(
                        "Image not found",
                        "Invalid image ID");
                }

                if (altText != null)
                    image.AltText = altText;

                if (displayOrder.HasValue)
                    image.DisplayOrder = displayOrder.Value;

                await _unitOfWork.Images.UpdateAsync(image, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var response = new ImageResponseDto
                {
                    ImageId = image.ImageId,
                    Url = image.Url,
                    AltText = image.AltText,
                    IsPrimary = image.IsPrimary,
                    DisplayOrder = image.DisplayOrder
                };

                return ServiceResponse<ImageResponseDto>.SuccessResponse(
                    response,
                    "Image updated successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<ImageResponseDto>.ErrorResponse(
                    "Failed to update image",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> SetPrimaryImageAsync(
            int imageId,
            int carId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var success = await _unitOfWork.Images.SetPrimaryImageAsync(imageId, carId, cancellationToken);

                if (success)
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return ServiceResponse<bool>.SuccessResponse(
                        true,
                        "Primary image set successfully");
                }

                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to set primary image",
                    "Image not found");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to set primary image",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> ReorderImagesAsync(
            int carId,
            Dictionary<int, int> imageIdToDisplayOrder,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var images = await _unitOfWork.Images.GetImagesByCarIdAsync(carId, cancellationToken);

                foreach (var image in images)
                {
                    if (imageIdToDisplayOrder.ContainsKey(image.ImageId))
                    {
                        image.DisplayOrder = imageIdToDisplayOrder[image.ImageId];
                        await _unitOfWork.Images.UpdateAsync(image, cancellationToken);
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Images reordered successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to reorder images",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> DeleteImageAsync(
            int imageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var image = await _unitOfWork.Images.GetByIdAsync(imageId, cancellationToken);
                if (image == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Image not found",
                        "Invalid image ID");
                }

                // Delete physical file (implement actual file deletion)
                await DeleteImageFileAsync(image.Url, cancellationToken);

                await _unitOfWork.Images.DeleteAsync(imageId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Image deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to delete image",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> DeleteImagesByCarIdAsync(
            int carId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var images = await _unitOfWork.Images.GetImagesByCarIdAsync(carId, cancellationToken);

                foreach (var image in images)
                {
                    await DeleteImageFileAsync(image.Url, cancellationToken);
                }

                await _unitOfWork.Images.DeleteImagesByCarIdAsync(carId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "All images deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to delete images",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> ValidateImageFileAsync(
            IFormFile file,
            CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask; // For async consistency

            if (file == null || file.Length == 0)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    "File is empty or not provided");
            }

            if (file.Length > _maxFileSize)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    $"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    $"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
            }

            return ServiceResponse<bool>.SuccessResponse(true, "File is valid");
        }

        private async Task<string> SaveImageFileAsync(
            IFormFile file,
            string fileName,
            CancellationToken cancellationToken)
        {
            // TODO: Implement actual file storage (local eller Azure Blob,.)
            // For now, return a placeholder URL
            await Task.CompletedTask;
            return $"/images/cars/{fileName}";
        }

        private async Task DeleteImageFileAsync(
            string imageUrl,
            CancellationToken cancellationToken)
        {
            // TODO: Implement actual file deletion
            await Task.CompletedTask;
        }
    }
}