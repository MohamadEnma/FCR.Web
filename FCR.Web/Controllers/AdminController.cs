using FCR.Dal.Classes;
using FCR.Dal.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using FCR.Dal.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FCR.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IMapper _mapper;

        public AdminController(
           UserManager<ApplicationUser> userManager,
           RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        ILogger<AdminController> logger, IMapper mapper
           )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
            _mapper = mapper;

        }

        // GET: Admin/Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        //GET: User/Users 
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = _mapper.Map<List<UserViewModel>>(users);

            // Populate roles separately
            foreach (var userViewModel in userViewModels)
            {
                var user = users.First(u => u.Id == userViewModel.Id);
                userViewModel.Roles = await _userManager.GetRolesAsync(user);
            }

            return View(userViewModels);
        }


        // GET: User/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new UserViewModel());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
                    return View(model);
                }

                var user = _mapper.Map<ApplicationUser>(model);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Created new user with ID {UserId}", user.Id);

                    // Handle role assignment
                    if (model.IsAdmin)
                    {
                        // Ensure Admin role exists
                        if (!await _roleManager.RoleExistsAsync("Admin"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Admin"));
                        }

                        await _userManager.AddToRoleAsync(user, "Admin");
                        _logger.LogInformation("Assigned Admin role to user {UserId}", user.Id);
                    }

                    TempData["SuccessMessage"] = "User created successfully!";
                    return RedirectToAction(nameof(Users));
                }

                // Handle password requirements explicitly
                foreach (var error in result.Errors)
                {
                    if (error.Code.Contains("Password"))
                    {
                        ModelState.AddModelError(nameof(model.Password), error.Description);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    _logger.LogWarning("User creation failed: {Error}", error.Description);
                }
            }

            return View(model);
        }


        // GET: User/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound();
            }
            var model = _mapper.Map<UserViewModel>(user);
            model.IsAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // Update basic info
            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            // Update password only if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            // Handle role changes
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (model.IsAdmin && !isAdmin)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            else if (!model.IsAdmin && isAdmin)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }

            return RedirectToAction(nameof(Users));
        }


        // GET: User/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            if (id == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _logger.LogWarning("Attempt to delete self with ID {UserId}", id);
                ModelState.AddModelError(string.Empty, "You cannot delete your own account.");
                return RedirectToAction(nameof(Users));
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound();
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Deleted user with ID {UserId}", id);
                return RedirectToAction(nameof(Users));
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
                _logger.LogWarning("User deletion failed: {Error}", error.Description);
            }
            return RedirectToAction(nameof(Users));
        }
    }
}
