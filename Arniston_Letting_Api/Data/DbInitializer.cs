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

        if (!await context.Users.AnyAsync())
        {
            context.Users.Add(new User
            {
                Email = "admin@arnistonletting.co.za",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                IsActive = true
            });
        }

        if (!await context.Owners.AnyAsync())
        {
            context.Owners.AddRange(
                new Owner
                {
                    FullName = "John Smith",
                    Email = "john@example.com",
                    PhoneNumber = "0820000001",
                    PreferredContactMethod = "Email",
                    Notes = "Property owner"
                },
                new Owner
                {
                    FullName = "Sarah Williams",
                    Email = "sarah@example.com",
                    PhoneNumber = "0820000002",
                    PreferredContactMethod = "Phone",
                    Notes = "Property owner"
                }
            );
        }

        await context.SaveChangesAsync();

        if (!await context.Properties.AnyAsync())
        {
            var owners = await context.Owners.ToListAsync();

            context.Properties.AddRange(
                new Property
                {
                    PropertyName = "Arniston Beach House",
                    Address = "Arniston, Western Cape",
                    Occupied = false,
                    OwnerId = owners[0].OwnerId,
                    Bedrooms = 3,
                    Sleeps = 6,
                    RatePerNight = 2500,
                    Description = "Holiday house near the beach.",
                    Parking = true,
                    Pool = false
                },
                new Property
                {
                    PropertyName = "Seaside Cottage",
                    Address = "Arniston, Western Cape",
                    Occupied = false,
                    OwnerId = owners[1].OwnerId,
                    Bedrooms = 2,
                    Sleeps = 4,
                    RatePerNight = 1800,
                    Description = "Comfortable holiday cottage.",
                    Parking = true,
                    Pool = false
                }
            );
        }

        if (!await context.Cleaners.AnyAsync())
        {
            context.Cleaners.AddRange(
                new Cleaner
                {
                    FullName = "Jane Cleaner",
                    PhoneNumber = "0820000010",
                    Email = "jane@example.com",
                    Available = true
                },
                new Cleaner
                {
                    FullName = "Peter Cleaner",
                    PhoneNumber = "0820000011",
                    Email = "peter@example.com",
                    Available = true
                }
            );
        }

        await context.SaveChangesAsync();

        if (!await context.Bookings.AnyAsync())
        {
            var properties = await context.Properties.ToListAsync();

            context.Bookings.AddRange(
                new Booking
                {
                    PropertyId = properties[0].PropertyId,
                    BookerName = "Michael Brown",
                    CheckIn = DateTime.Today.AddDays(7),
                    CheckOut = DateTime.Today.AddDays(12),
                    Rate = 2500
                },
                new Booking
                {
                    PropertyId = properties[1].PropertyId,
                    BookerName = "Emily Jones",
                    CheckIn = DateTime.Today.AddDays(14),
                    CheckOut = DateTime.Today.AddDays(18),
                    Rate = 1800
                }
            );
        }

        await context.SaveChangesAsync();

        if (!await context.CleanerTasks.AnyAsync())
        {
            var cleaners = await context.Cleaners.ToListAsync();
            var properties = await context.Properties.ToListAsync();

            context.CleanerTasks.AddRange(
                new CleanerTask
                {
                    CleanerId = cleaners[0].CleanerId,
                    LocationId = properties[0].PropertyId,
                    Date = DateTime.Today.AddDays(7),
                    Time = new TimeSpan(10, 0, 0),
                    Completed = false,
                    Notes = "Full house cleaning"
                },
                new CleanerTask
                {
                    CleanerId = cleaners[1].CleanerId,
                    LocationId = properties[1].PropertyId,
                    Date = DateTime.Today.AddDays(14),
                    Time = new TimeSpan(10, 0, 0),
                    Completed = false,
                    Notes = "Standard cleaning"
                }
            );
        }

        await context.SaveChangesAsync();

        if (!await context.Breakages.AnyAsync())
        {
            var properties = await context.Properties.ToListAsync();

            context.Breakages.Add(new Breakage
            {
                LocationId = properties[0].PropertyId,
                Date = DateTime.Today,
                Time = DateTime.Now.TimeOfDay,
                ReportedBy = "Admin",
                Notes = "Example breakage report.",
                Resolved = false
            });
        }

        if (!await context.Notifications.AnyAsync())
        {
            context.Notifications.AddRange(
                new Notification
                {
                    Type = "Booking",
                    Recipient = "Admin",
                    Date = DateTime.Today,
                    Time = DateTime.Now.TimeOfDay,
                    Message = "New booking received.",
                    Status = "Pending"
                },
                new Notification
                {
                    Type = "Cleaner Task",
                    Recipient = "Admin",
                    Date = DateTime.Today,
                    Time = DateTime.Now.TimeOfDay,
                    Message = "Cleaner task requires attention.",
                    Status = "Sent"
                }
            );
        }

        if (!await context.Reports.AnyAsync())
        {
            context.Reports.AddRange(
                new Report
                {
                    ReportType = "Bookings",
                    Date = DateTime.Today,
                    GeneratedBy = "Admin",
                    Status = "Completed",
                    Description = "Booking report."
                },
                new Report
                {
                    ReportType = "Properties",
                    Date = DateTime.Today,
                    GeneratedBy = "Admin",
                    Status = "Completed",
                    Description = "Property report."
                }
            );
        }

        await context.SaveChangesAsync();
    }
}