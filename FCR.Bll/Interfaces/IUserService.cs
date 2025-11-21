using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.User;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Interfaces
{
    public interface IUserService
    {
        // Get User Info
        Task<ServiceResponse<UserProfileDto>> GetUserProfileAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<UserDto>> GetUserByIdAsync(
            string userId,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<IEnumerable<UserDto>>> GetAllUsersAsync(
            CancellationToken cancellationToken = default);  // Admin only

        // Update Profile
        Task<ServiceResponse<UserProfileDto>> UpdateProfileAsync(
            string userId,
            UpdateProfileDto updateDto,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<UserDto>> UpdateUserAsync(
            string userId,
            UpdateUserDto updateDto,
            CancellationToken cancellationToken = default);  // Admin only

        // Delete Account
        Task<ServiceResponse<bool>> DeleteAccountAsync(
            string userId,
            DeleteAccountDto deleteDto,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> ChangeEmailAsync(
           string userId,
           string newEmail,
           string password,
           CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> AdminDeleteUserAsync(
            string userId,
            CancellationToken cancellationToken = default);
    }
}