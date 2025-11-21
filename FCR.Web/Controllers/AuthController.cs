using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FCR.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IClient _apiClient;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IClient apiClient, ILogger<AuthController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiClient.LoginAsync(model);

                if (response?.Success == true && response.Data != null)
                {
                    var loginData = response.Data;

                    // Parse JWT token
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(loginData.Token);

                    // Create claims from JWT
                    var claims = jwtToken.Claims.ToList();

                    var claimsIdentity = new ClaimsIdentity(
                        claims,
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = jwtToken.ValidTo
                    };

                    // Store JWT token for API calls
                    authProperties.StoreTokens(new List<AuthenticationToken>
                    {
                new AuthenticationToken
                {
                    Name = "access_token",
                    Value = loginData.Token
                }
            });

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    //  DEBUG: Verify token was saved
                    var savedToken = await HttpContext.GetTokenAsync("access_token");
                    _logger.LogInformation("Token saved: {TokenExists}", !string.IsNullOrEmpty(savedToken));
                    _logger.LogInformation("Token length: {TokenLength}", savedToken?.Length ?? 0);
                    _logger.LogInformation("User {Email} logged in successfully", model.Email);

                    TempData["SuccessMessage"] = $"Welcome back!";

                    var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    if (role == "Admin")
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", response?.Message ?? "Invalid email or password.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        // GET: Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiClient.RegisterAsync(model);

                if (response?.Success == true)
                {
                    TempData["SuccessMessage"] = "Registration successful! Please login.";
                    return RedirectToAction(nameof(Login));
                }
                else
                {
                    ModelState.AddModelError("", response?.Message ?? "Registration failed.");
                    return View(model);
                }
            }
            catch (ApiException ex)
            {
                _logger.LogWarning(ex, "Registration failed for user {Email}", model.Email);
                ModelState.AddModelError("", ex.Message ?? "Registration failed. Please try again.");
                return View(model);
            }
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Remove JWT cookie
            Response.Cookies.Delete("jwt");

            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}