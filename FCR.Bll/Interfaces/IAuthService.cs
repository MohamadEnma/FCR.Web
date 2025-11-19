using FCR.Bll.Common;
using FCR.Bll.DTOs.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Interfaces
{
    public interface IAuthService
    {
        // Registration
        Task<ServiceResponse<LoginResponseDto>> RegisterAsync(
            RegisterDto registerDto,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<LoginResponseDto>> RegisterAdminAsync(
            RegisterDto registerDto,
            CancellationToken cancellationToken = default);

        // Login
        Task<ServiceResponse<LoginResponseDto>> LoginAsync(
            LoginDto loginDto,
            CancellationToken cancellationToken = default);

        // Token Management
        Task<ServiceResponse<LoginResponseDto>> RefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default);

        // Logout
        Task<ServiceResponse<bool>> LogoutAsync(
            string userId,
            CancellationToken cancellationToken = default);

        // Password Management
        Task<ServiceResponse<bool>> ChangePasswordAsync(
            string userId,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> ForgotPasswordAsync(
            string email,
            CancellationToken cancellationToken = default);

        Task<ServiceResponse<bool>> ResetPasswordAsync(
            string email,
            string token,
            string newPassword,
            CancellationToken cancellationToken = default);

        // Email Confirmation (optional - for production)
        Task<ServiceResponse<bool>> ConfirmEmailAsync(
            string userId,
            string token,
            CancellationToken cancellationToken = default);
    }
}