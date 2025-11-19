using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Auth;
using FCR.Bll.DTOs.User;
using FCR.Bll.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FCR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<LoginResponseDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _authService.RegisterAsync(model);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("User registered successfully: {Email}", model.Email);
            return Ok(result);
        }

        /// <summary>
        /// Register a new admin user (Admin only)
        /// </summary>
        [HttpPost("register-admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<LoginResponseDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _authService.RegisterAdminAsync(model);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Admin registered successfully: {Email}", model.Email);
            return Ok(result);
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<LoginResponseDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _authService.LoginAsync(model);

            if (!result.Success)
                return Unauthorized(result);

            _logger.LogInformation("User logged in successfully: {Email}", model.Email);
            return Ok(result);
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<bool>.ErrorResponse("Unauthorized", "User not found"));

            var result = await _authService.LogoutAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("User logged out successfully: {UserId}", userId);
            return Ok(result);
        }

        /// <summary>
        /// Change password for authenticated user
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<bool>.ErrorResponse("Unauthorized", "User not found"));

            var result = await _authService.ChangePasswordAsync(
                userId,
                model.CurrentPassword,
                model.NewPassword);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return Ok(result);
        }

        /// <summary>
        /// Request password reset (sends email)
        /// </summary>
        [HttpPost("forgot-password")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _authService.ForgotPasswordAsync(model.Email);

            // Always return success for security (don't reveal if email exists)
            return Ok(result);
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _authService.ResetPasswordAsync(
                model.Email,
                model.Token,
                model.NewPassword);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Password reset successfully for: {Email}", model.Email);
            return Ok(result);
        }

        /// <summary>
        /// Confirm email with token
        /// </summary>
        [HttpGet("confirm-email")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Invalid request",
                    "User ID and token are required"));

            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Email confirmed successfully for user: {UserId}", userId);
            return Ok(result);
        }

        /// <summary>
        /// Get current user info (for testing authentication)
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            return Ok(new
            {
                UserId = userId,
                Email = email,
                Name = name,
                Roles = roles,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false
            });
        }

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<LoginResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<LoginResponseDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var result = await _authService.RefreshTokenAsync(dto.RefreshToken);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("Token refreshed successfully");
            return Ok(result);
        }

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        [HttpPost("revoke-token")]
        [Authorize]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RevokeToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _authService.LogoutAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}