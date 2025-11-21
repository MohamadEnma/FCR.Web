using Polly;
using Polly.Extensions.Http;
using FCR.Web.Services;
using FCR.Web.Services.Base;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace FCR.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // Add HttpContextAccessor first
            builder.Services.AddHttpContextAccessor();

            // Register token handler
            builder.Services.AddTransient<AuthTokenHandler>();

            // Register CarViewService
            builder.Services.AddScoped<ICarViewService, CarViewService>();

            var retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            // Register NSwag client with retry policy
            builder.Services.AddHttpClient<IClient, Client>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7172");
                client.Timeout = TimeSpan.FromSeconds(120); // Increase from 30 to 120 seconds
            })
            .AddHttpMessageHandler<AuthTokenHandler>()
            .AddPolicyHandler(retryPolicy) 
            .AddPolicyHandler(retryPolicy) 
            .AddHttpMessageHandler<AuthTokenHandler>();

            // Cookie authentication only (no JWT bearer for web app)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(24);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    
                });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "adminCars",
                pattern: "Admin/Cars/{action=Index}/{id?}",
                defaults: new { controller = "AdminCars" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}