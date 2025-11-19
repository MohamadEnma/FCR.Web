using FCR.Bll.Common;
using FCR.Bll.DTOs;
using FCR.Bll.DTOs.Auth;
using FCR.Bll.Interfaces;
using FCR.Dal.Classes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<LoginResponseDto>> RegisterAsync(
            RegisterDto registerDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return ServiceResponse<LoginResponseDto>.ErrorResponse(
                        "Registration failed",
                        "Email is already registered");
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<LoginResponseDto>.ErrorResponse(
                        "Registration failed",
                        errors);
                }

                await _userManager.AddToRoleAsync(user, "User");

                var token = GenerateJwtToken(user, new List<string> { "User" });
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);
                var response = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = new List<string> { "User" }
                };

                return ServiceResponse<LoginResponseDto>.SuccessResponse(
                    response,
                    "Registration successful");
            }
            catch (Exception ex)
            {
                return ServiceResponse<LoginResponseDto>.ErrorResponse(
                    "Registration failed",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<LoginResponseDto>> RegisterAdminAsync(
            RegisterDto registerDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    return ServiceResponse<LoginResponseDto>.ErrorResponse(
                        "Registration failed",
                        "Email is already registered");
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<LoginResponseDto>.ErrorResponse(
                        "Registration failed",
                        errors);
                }

                await _userManager.AddToRoleAsync(user, "Admin");

                var token = GenerateJwtToken(user, new List<string> { "Admin" });

                var response = new LoginResponseDto
                {
                    Token = token,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = new List<string> { "Admin" }
                };

                return ServiceResponse<LoginResponseDto>.SuccessResponse(
                    response,
                    "Admin registration successful");
            }
            catch (Exception ex)
            {
                return ServiceResponse<LoginResponseDto>.ErrorResponse(
                    "Registration failed",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(
            LoginDto loginDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return ServiceResponse<LoginResponseDto>.ErrorResponse(
                        "Login failed",
                        "Invalid email or password");
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
                if (!isPasswordValid)
                {
                    return ServiceResponse<LoginResponseDto>.ErrorResponse(
                        "Login failed",
                        "Invalid email or password");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, roles.ToList());
                var refreshToken = GenerateRefreshToken();
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1); // 1 days validity
                await _userManager.UpdateAsync(user);

                var response = new LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList(),
                    TokenExpiration = DateTime.UtcNow.AddHours(24)
                };

                return ServiceResponse<LoginResponseDto>.SuccessResponse(
                    response,
                    "Login successful");
            }
            catch (Exception ex)
            {
                return ServiceResponse<LoginResponseDto>.ErrorResponse(
                    "Login failed",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> LogoutAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Logout failed",
                        "User not found");
                }

                await _userManager.UpdateSecurityStampAsync(user);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Logout successful");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Logout failed",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> ChangePasswordAsync(
            string userId,
            string currentPassword,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Change password failed",
                        "User not found");
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<bool>.ErrorResponse(
                        "Change password failed",
                        errors);
                }

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Password changed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Change password failed",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> ForgotPasswordAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ServiceResponse<bool>.SuccessResponse(
                        true,
                        "If the email exists, a password reset link has been sent");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Password reset token generated");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Forgot password failed",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(
            string email,
            string token,
            string newPassword,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Reset password failed",
                        "User not found");
                }

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<bool>.ErrorResponse(
                        "Reset password failed",
                        errors);
                }

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Password reset successful");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Reset password failed",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> ConfirmEmailAsync(
            string userId,
            string token,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Email confirmation failed",
                        "User not found");
                }

                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<bool>.ErrorResponse(
                        "Email confirmation failed",
                        errors);
                }

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Email confirmed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Email confirmation failed",
                    ex.Message);
            }
        }

        private string GenerateJwtToken(ApplicationUser user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ServiceResponse<LoginResponseDto>> RefreshTokenAsync(
           string refreshToken,
           CancellationToken cancellationToken = default)
        {
            try
            {
                // Find user by refresh token
                var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);

                if (user == null)
                {
                    return ServiceResponse<LoginResponseDto>.ErrorResponse(
                        "Invalid token",
                        "Refresh token not found");
                }

                // Check if refresh token has expired
                if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    return ServiceResponse<LoginResponseDto>.ErrorResponse(
                        "Token expired",
                        "Refresh token has expired");
                }

                // Generate new tokens
                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = GenerateJwtToken(user, roles.ToList());
                var newRefreshToken = GenerateRefreshToken();

                // Update refresh token in database
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days validity

                await _userManager.UpdateAsync(user);

                var response = new LoginResponseDto
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                };

                return ServiceResponse<LoginResponseDto>.SuccessResponse(
                    response,
                    "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<LoginResponseDto>.ErrorResponse(
                    "Token refresh failed",
                    ex.Message);
            }
        }

        //  Helper method to generate refresh token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
      
    }
}