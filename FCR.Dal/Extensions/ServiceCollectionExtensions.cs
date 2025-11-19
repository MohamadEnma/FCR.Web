using FCR.Dal.Classes;
using FCR.Dal.Data;
using FCR.Dal.Repositories.Implementation;
using FCR.Dal.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FCR.Dal.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccessLayer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Register DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()));


            // 3. Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 4. Register Repositories
            services.AddScoped<ICarRepository, CarRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IImageRepository, ImageRepository>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            return services;
        }
    }
}