using Arniston_Letting_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Data;

public static class DbInitializer
{
    // Development / demonstration defaults. These are only used when no
    // Seed:AdminEmail / Seed:AdminPassword configuration is supplied, which
    // keeps existing setups working exactly as before.
    private const string DefaultAdminEmail = "admin@arnistonletting.co.za";
    private const string DefaultAdminPassword = "Admin123!";

    // Matches the minimum length enforced by the login request, otherwise
    // the seeded administrator would never be able to sign in.
    private const int MinimumPasswordLength = 8;

    public static async Task InitializeAsync(
        ApplicationDbContext context,
        IConfiguration? configuration = null,
        IHostEnvironment? environment = null,
        ILogger? logger = null)
    {
        if (context.Database.GetMigrations().Any())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        await CreateAdminUser(context, configuration, environment, logger);
    }

    private static async Task CreateAdminUser(
        ApplicationDbContext context,
        IConfiguration? configuration,
        IHostEnvironment? environment,
        ILogger? logger)
    {
        // Only seed an administrator into an empty Users table.
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var configuredEmail = configuration?["Seed:AdminEmail"];
        var configuredPassword = configuration?["Seed:AdminPassword"];

        var email = string.IsNullOrWhiteSpace(configuredEmail)
            ? DefaultAdminEmail
            : configuredEmail.Trim().ToLowerInvariant();

        var usingDefaultPassword = string.IsNullOrWhiteSpace(configuredPassword);

        var password = usingDefaultPassword
            ? DefaultAdminPassword
            : configuredPassword!;

        if (password.Length < MinimumPasswordLength)
        {
            throw new InvalidOperationException(
                $"Seed:AdminPassword must be at least {MinimumPasswordLength} characters long.");
        }

        if (usingDefaultPassword && environment?.IsDevelopment() != true)
        {
            logger?.LogWarning(
                "Seeding the administrator account with the publicly documented " +
                "default password. Set Seed__AdminPassword before first start in " +
                "any non-development environment, or change the password immediately.");
        }

        var admin = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = "Admin",
            LastName = "User",
            Role = "Admin",
            IsActive = true
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();
    }
}