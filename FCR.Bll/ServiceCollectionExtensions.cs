using FCR.Bll.Interfaces;
using FCR.Bll.Mappings;
using FCR.Bll.Services;
using FCR.Bll.Validators;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FCR.Bll.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
            // Register all BLL services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICarService, CarService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IUserService, UserService>();

            // Register FluentValidation validators
            services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();

            // Register Mapster
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetExecutingAssembly());

            // Apply custom configurations
            MapsterConfig.Configure();

            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }
    }
}