using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.User;
using FCR.Bll.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FCR.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All endpoints require authentication
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserService userService,
            IAuthService authService,
            ILogger<UserController> logger)
        {
            _userService = userService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user's profile
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ServiceResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ServiceResponse<UserProfileDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<UserProfileDto>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _userService.GetUserProfileAsync(userId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Update current user's profile
        /// </summary>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(ServiceResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<UserProfileDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<UserProfileDto>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<UserProfileDto>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _userService.UpdateProfileAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("User {UserId} updated their profile", userId);
            return Ok(result);
        }

        /// <summary>
        /// Change current user's password
        /// </summary>
        [HttpPut("change-password")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<bool>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _authService.ChangePasswordAsync(
                userId,
                dto.CurrentPassword,
                dto.NewPassword);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("User {UserId} changed their password", userId);
            return Ok(result);
        }

        /// <summary>
        /// Change current user's email
        /// </summary>
        [HttpPut("change-email")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<bool>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _userService.ChangeEmailAsync(userId, dto.NewEmail, dto.Password);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogInformation("User {UserId} changed their email to {NewEmail}", userId, dto.NewEmail);
            return Ok(result);
        }

        /// <summary>
        /// Delete current user's account
        /// </summary>
        [HttpDelete("account")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResponse<bool>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteMyAccount([FromBody] DeleteAccountDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ServiceResponse<bool>.ErrorResponse(
                    "Validation failed",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<bool>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _userService.DeleteAccountAsync(userId, dto);

            if (!result.Success)
                return BadRequest(result);

            _logger.LogWarning("User {UserId} deleted their account", userId);
            return Ok(result);
        }

        /// <summary>
        /// Get current user's booking statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ServiceResponse<UserProfileDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyStatistics()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ServiceResponse<UserProfileDto>.ErrorResponse(
                    "Unauthorized",
                    "User not found"));

            var result = await _userService.GetUserProfileAsync(userId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}