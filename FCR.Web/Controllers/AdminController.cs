using Azure;
using FCR.Bll.Common;
using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCR.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IClient _apiClient;
        private readonly ILogger<AdminController> _logger;
    

        public AdminController(IClient apiClient, ILogger<AdminController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }


        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                // ✅ Response is AdminStatisticsDto directly
                var statistics = await _apiClient.StatisticsAsync();

                if (statistics != null)
                {
                    _logger.LogInformation(
                        "Statistics loaded: TotalUsers={TotalUsers}, TotalCars={TotalCars}, TotalBookings={TotalBookings}, TotalRevenue={TotalRevenue}",
                        statistics.TotalUsers,
                        statistics.TotalCars,
                        statistics.TotalBookings,
                        statistics.TotalRevenue);

                    return View(statistics);
                }
                else
                {
                    _logger.LogWarning("Statistics returned null");
                    TempData["ErrorMessage"] = "Unable to load dashboard statistics.";

                    return View(new AdminStatisticsDto
                    {
                        TotalUsers = 0,
                        TotalBookings = 0,
                        TotalCars = 0,
                        TotalRevenue = 0
                    });
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error loading dashboard: StatusCode={StatusCode}", ex.StatusCode);

                TempData["ErrorMessage"] = ex.StatusCode == 401
                    ? "Unauthorized. Please login again."
                    : "Unable to load dashboard.";

                return View(new AdminStatisticsDto
                {
                    TotalUsers = 0,
                    TotalBookings = 0,
                    TotalCars = 0,
                    TotalRevenue = 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error loading dashboard");
                TempData["ErrorMessage"] = "An error occurred.";

                return View(new AdminStatisticsDto
                {
                    TotalUsers = 0,
                    TotalBookings = 0,
                    TotalCars = 0,
                    TotalRevenue = 0
                });
            }
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            try
            {
                var response = await _apiClient.UsersGETAsync();
                var users = response?.Data ?? new List<UserDto>();
                return View(users);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "Unable to load users. Please try again.";
                return View(new List<UserDto>());
            }
        }

        // GET: Admin/ManageCars
        public async Task<IActionResult> ManageCars()
        {
            try
            {
                var response = await _apiClient.CarsGETAsync();

                if (response?.Data != null)
                {
                    return View(response.Data);
                }

                return View(new List<CarResponseDto>());
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading cars for management");
                ViewBag.ErrorMessage = "Unable to load cars.";
                return View(new List<CarResponseDto>());
            }
        }

        // POST: Admin/DeleteCar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCar(int id)
        {
            try
            {
                await _apiClient.CarsDELETEAsync(id);
                TempData["SuccessMessage"] = "Car deleted successfully.";
                return RedirectToAction(nameof(ManageCars));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error deleting car");
                TempData["ErrorMessage"] = "Unable to delete car.";
                return RedirectToAction(nameof(ManageCars));
            }
        }

        // GET: Admin/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new RegisterDto());
        }


        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterDto model, bool isAdmin = false)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Use RegisterAdmin if isAdmin is true
                var response = isAdmin
                    ? await _apiClient.RegisterAdminAsync(model)
                    : await _apiClient.RegisterAsync(model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    ModelState.AddModelError("", response?.Message ?? "Failed to create user.");
                    return View(model);
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", ex.Message ?? "Error creating user. Please try again.");
                return View(model);
            }
        }

        // GET: Admin/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var response = await _apiClient.UsersGET2Async(id);

                if (response?.Data == null)
                {
                    return NotFound();
                }

                // Pass data via ViewBag
                ViewBag.UserId = response.Data.Id;
                ViewBag.Email = response.Data.Email;
                ViewBag.FirstName = response.Data.FirstName;
                ViewBag.LastName = response.Data.LastName;
                ViewBag.IsAdmin = response.Data.Roles?.Contains("Admin") ?? false;

                return View();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading user for edit");
                return NotFound();
            }
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string firstName, string lastName, string? password, bool isAdmin = false)
        {
            try
            {
                // Create update DTO
                var updateDto = new UpdateUserDto
                {
                    FirstName = firstName,
                    LastName = lastName
                };

                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(password))
                {
                    // placeholder -------------> API endpoint for password update
                   
                }

                // Update user
                var response = await _apiClient.UsersPUTAsync(id, updateDto);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "User updated successfully!";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to update user.";
                    return RedirectToAction(nameof(Edit), new { id });
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error updating user");
                TempData["ErrorMessage"] = "Error updating user. Please try again.";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        // GET: Admin/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var response = await _apiClient.UsersGET2Async(id);

                if (response?.Data == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction(nameof(Users));
                }

                return View(response.Data);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error loading user for deletion");
                TempData["ErrorMessage"] = "Unable to load user details.";
                return RedirectToAction(nameof(Users));
            }
        }

        // POST: Admin/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var response = await _apiClient.UsersDELETEAsync(id);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "User deleted successfully.";
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    TempData["ErrorMessage"] = response?.Message ?? "Failed to delete user.";
                    return RedirectToAction(nameof(Users));
                }
            }
            catch (ApiException ex) when (ex.StatusCode == 400)
            {
                _logger.LogError(ex, "Error deleting user - bad request");
                TempData["ErrorMessage"] = "Cannot delete this user. You may be trying to delete your own account.";
                return RedirectToAction(nameof(Users));
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error deleting user");
                TempData["ErrorMessage"] = "Error deleting user. Please try again.";
                return RedirectToAction(nameof(Users));
            }
        }

    }
}