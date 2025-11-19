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
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiClient.LoginAsync(model);

                // Parse JWT token and create claims
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(response.Token);

                var claims = jwtToken.Claims.ToList();
                claims.Add(new Claim("AccessToken", response.Token));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = jwtToken.ValidTo
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["SuccessMessage"] = "Login successful!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (ApiException ex)
            {
                _logger.LogWarning(ex, "Login failed for user {Email}", model.Email);
                ModelState.AddModelError("", "Invalid email or password.");
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

                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction(nameof(Login));
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
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }
    }
}