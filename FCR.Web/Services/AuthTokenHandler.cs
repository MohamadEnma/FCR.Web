using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace FCR.Web.Services
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthTokenHandler> _logger;

        public AuthTokenHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthTokenHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                // ✅ Try to get token
                var token = await httpContext.GetTokenAsync("access_token");

                // ✅ DEBUG: Log token status
                _logger.LogInformation("AuthTokenHandler - User authenticated: {IsAuthenticated}",
                    httpContext.User?.Identity?.IsAuthenticated ?? false);
                _logger.LogInformation("AuthTokenHandler - Token found: {TokenExists}",
                    !string.IsNullOrEmpty(token));
                _logger.LogInformation("AuthTokenHandler - Token length: {TokenLength}",
                    token?.Length ?? 0);

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("AuthTokenHandler - Authorization header added");
                }
                else
                {
                    _logger.LogWarning("AuthTokenHandler - No token found!");
                }
            }
            else
            {
                _logger.LogWarning("AuthTokenHandler - HttpContext is null!");
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}