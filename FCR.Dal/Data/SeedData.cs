using FCR.Dal.Classes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FCR.Dal.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ========== SEED ROLES ==========
        
            string[] roleNames = { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"Role '{roleName}' created");
                }
                else
                {
                    Console.WriteLine($" Role '{roleName}' already exists");
                }
            }

            // ========== SEED ADMIN USER ==========
            
            var adminEmail = "fredrik@fcr.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Fredrik",
                    LastName = "Terent",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                    Console.WriteLine($"Admin user created: {adminEmail} / Admin@123");
                }
                else
                {
                    Console.WriteLine($" Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"  Admin user already exists: {adminEmail}");
            }

            // ========== SEED TEST USER ==========
        
            var testEmail = "User@fcr.se";
            var testUser = await userManager.FindByEmailAsync(testEmail);

            if (testUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = testEmail,
                    Email = testEmail,
                    FirstName = "User",
                    LastName = "System",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "User@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    Console.WriteLine($" Test user created: {testEmail} / User@123");
                }
                else
                {
                    Console.WriteLine($" Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($" Test user already exists: {testEmail}");
            }

            // ========== SEED SAMPLE CARS ==========
     
            if (!context.Cars.Any())
            {
                var cars = new[]
{
    new Car
    {
        Brand = "Toyota",
        ModelName = "Camry",
        Category = "Sedan",
        Year = 2024,
        DailyRate = 45.00m,
        IsAvailable = true,
        Transmission = "Automatic",
        FuelType = "Petrol",
        Seats = 5, 
        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        
    },
    new Car
    {
        Brand = "Honda",
        ModelName = "Civic",
        Category = "Sedan",
        Year = 2023,
        DailyRate = 40.00m,
        IsAvailable = true,
        Transmission = "Automatic",
        FuelType = "Petrol",
        Seats = 5,
        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Car
    {
        Brand = "BMW",
        ModelName = "X5",
        Category = "SUV",
        Year = 2024,
        DailyRate = 85.00m,
        IsAvailable = true,
        Transmission = "Automatic",
        FuelType = "Diesel",
        Seats = 7,
        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Car
    {
        Brand = "Mercedes",
        ModelName = "C-Class",
        Category = "Luxury",
        Year = 2024,
        DailyRate = 95.00m,
        IsAvailable = true,
        Transmission = "Automatic",
        FuelType = "Petrol",
        Seats = 5,
        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Car
    {
        Brand = "Ford",
        ModelName = "Mustang",
        Category = "Sports",
        Year = 2023,
        DailyRate = 120.00m,
        IsAvailable = true,
        Transmission = "Manual",
        FuelType = "Petrol",
        Seats = 4,
        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    },
    new Car
    {
        Brand = "Tesla",
        ModelName = "Model 3",
        Category = "Electric",
        Year = 2024,
        DailyRate = 110.00m,
        IsAvailable = true,
        Transmission = "Automatic",
        FuelType = "Electric",
        Seats = 5,
        CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        UpdatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    }
};

                context.Cars.AddRange(cars);
                await context.SaveChangesAsync();
                Console.WriteLine($"✅ {cars.Length} sample cars created");
            }
            else
            {
                Console.WriteLine($"⏭️  Sample cars already exist ({context.Cars.Count()} cars in database)");
            }

            Console.WriteLine("\n✅ Database seeding completed!\n");
        }
    }
}