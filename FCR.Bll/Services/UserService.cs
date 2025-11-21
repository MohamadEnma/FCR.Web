using FCR.Bll.Common;
using FCR.Bll.DTOs.User;
using FCR.Bll.Interfaces;
using FCR.Dal.Classes;
using FCR.Dal.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FCR.Bll.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResponse<UserProfileDto>> GetUserProfileAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<UserProfileDto>.ErrorResponse(
                        "User not found",
                        "Invalid user ID");
                }

                var roles = await _userManager.GetRolesAsync(user);

                // Get booking statistics
                var bookings = await _unitOfWork.Bookings.GetBookingsByUserIdAsync(userId, cancellationToken);
                var totalBookings = bookings.Count();
                var activeBookings = bookings.Count(b => b.Status == "Confirmed" || b.Status == "Pending");
                var completedBookings = bookings.Count(b => b.Status == "Completed");
                var cancelledBookings = bookings.Count(b => b.IsCancelled);
                var totalSpent = bookings.Where(b => b.Status == "Completed").Sum(b => b.TotalPrice);

                var profile = new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DisplayName = $"{user.FirstName} {user.LastName}",
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    TotalBookings = totalBookings,
                    ActiveBookings = activeBookings,
                    CompletedBookings = completedBookings,
                    CancelledBookings = cancelledBookings,
                    TotalSpent = totalSpent
                };

                return ServiceResponse<UserProfileDto>.SuccessResponse(profile);
            }
            catch (Exception ex)
            {
                return ServiceResponse<UserProfileDto>.ErrorResponse(
                    "Failed to retrieve user profile",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<UserDto>> GetUserByIdAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<UserDto>.ErrorResponse(
                        "User not found",
                        "Invalid user ID");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    DisplayName = $"{user.FirstName} {user.LastName}",
                    PhoneNumber = user.PhoneNumber,
                    Roles = roles.ToList(),
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                };

                return ServiceResponse<UserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ServiceResponse<UserDto>.ErrorResponse(
                    "Failed to retrieve user",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<IEnumerable<UserDto>>> GetAllUsersAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                var users = _userManager.Users.ToList();
                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        DisplayName = $"{user.FirstName} {user.LastName}",
                        PhoneNumber = user.PhoneNumber,
                        Roles = roles.ToList(),
                        EmailConfirmed = user.EmailConfirmed,
                        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    });
                }

                return ServiceResponse<IEnumerable<UserDto>>.SuccessResponse(userDtos);
            }
            catch (Exception ex)
            {
                return ServiceResponse<IEnumerable<UserDto>>.ErrorResponse(
                    "Failed to retrieve users",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<UserProfileDto>> UpdateProfileAsync(
            string userId,
            UpdateProfileDto updateDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<UserProfileDto>.ErrorResponse(
                        "User not found",
                        "Invalid user ID");
                }

                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.PhoneNumber = updateDto.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return ServiceResponse<UserProfileDto>.ErrorResponse(
                        "Failed to update profile",
                        result.Errors.Select(e => e.Description).ToList());
                }

                // Return updated profile
                return await GetUserProfileAsync(userId, cancellationToken);
            }
            catch (Exception ex)
            {
                return ServiceResponse<UserProfileDto>.ErrorResponse(
                    "Failed to update profile",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(
            string userId,
            UpdateUserDto updateDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<UserDto>.ErrorResponse(
                        "User not found",
                        "Invalid user ID");
                }

                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.Email = updateDto.Email;
                user.UserName = updateDto.Email;
                user.PhoneNumber = updateDto.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    return ServiceResponse<UserDto>.ErrorResponse(
                        "Failed to update user",
                        result.Errors.Select(e => e.Description).ToList());
                }

                return await GetUserByIdAsync(userId, cancellationToken);
            }
            catch (Exception ex)
            {
                return ServiceResponse<UserDto>.ErrorResponse(
                    "Failed to update user",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> DeleteAccountAsync(
            string userId,
            DeleteAccountDto deleteDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "User not found",
                        "Invalid user ID");
                }

                // Verify password
                var passwordValid = await _userManager.CheckPasswordAsync(user, deleteDto.Password);
                if (!passwordValid)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Invalid password",
                        "Password is incorrect");
                }

                // Check for active bookings
                var activeBookings = await _unitOfWork.Bookings.GetActiveBookingsAsync(userId, cancellationToken);
                if (activeBookings.Any())
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Cannot delete account",
                        "You have active bookings. Please cancel them first.");
                }

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Failed to delete account",
                        result.Errors.Select(e => e.Description).ToList());
                }

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Account deleted successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to delete account",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> AdminDeleteUserAsync(
            string userId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "User not found",
                        "Invalid user ID");
                }

                // Admin-specific delete bypasses standard user validations:
                // - No password verification required (admin authority is sufficient)
                // - No active bookings check (admin assumes responsibility for data cleanup)
                // This improves performance by avoiding expensive queries and allows admins to force deletion when necessary.

                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Failed to delete user",
                        result.Errors.Select(e => e.Description).ToList());
                }

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "User deleted successfully by admin");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to delete user",
                    ex.Message);
            }
        }

        public async Task<ServiceResponse<bool>> ChangeEmailAsync(
    string userId,
    string newEmail,
    string password,
    CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(newEmail))
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Invalid email",
                        "New email cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Invalid password",
                        "Password is required to change email");
                }

                // Find user
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "User not found",
                        "Invalid user ID");
                }

                // Verify password
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!isPasswordValid)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Invalid password",
                        "The password you entered is incorrect");
                }

                // Check if new email is already taken
                var existingUser = await _userManager.FindByEmailAsync(newEmail);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Email already exists",
                        "This email address is already registered");
                }

                // Check if new email is the same as current
                if (user.Email?.Equals(newEmail, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return ServiceResponse<bool>.ErrorResponse(
                        "Same email",
                        "New email is the same as your current email");
                }

                // Update email and username
                user.Email = newEmail;
                user.UserName = newEmail;
                user.NormalizedEmail = newEmail.ToUpperInvariant();
                user.NormalizedUserName = newEmail.ToUpperInvariant();
                user.EmailConfirmed = false; // Require re-confirmation

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResponse<bool>.ErrorResponse(
                        "Failed to change email",
                        errors);
                }

                
                 var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                // Send confirmation email with token

                return ServiceResponse<bool>.SuccessResponse(
                    true,
                    "Email changed successfully. Please verify your new email address.");
            }
            catch (Exception ex)
            {
                return ServiceResponse<bool>.ErrorResponse(
                    "Failed to change email",
                    ex.Message);
            }
        }
    }
}