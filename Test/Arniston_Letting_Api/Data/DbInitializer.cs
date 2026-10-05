using Arniston_Letting_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        if (context.Database.GetMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        await CreateAdminUser(context);
    }

    private static async Task CreateAdminUser(ApplicationDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var admin = new User
        {
            Email = "admin@arnistonletting.co.za",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin",
            IsActive = true
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }
}