using Arniston_Letting_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Arniston_Letting_API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Owner> Owners { get; set; }

    public DbSet<Property> Properties { get; set; }

    public DbSet<Booking> Bookings { get; set; }

    public DbSet<Cleaner> Cleaners { get; set; }

    public DbSet<CleanerTask> CleanerTasks { get; set; }

    public DbSet<Breakage> Breakages { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Report> Reports { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasKey(u => u.UserId);

        modelBuilder.Entity<Owner>()
            .HasKey(o => o.OwnerId);

        modelBuilder.Entity<Property>()
            .HasKey(p => p.PropertyId);

        modelBuilder.Entity<Booking>()
            .HasKey(b => b.BookingId);

        modelBuilder.Entity<Cleaner>()
            .HasKey(c => c.CleanerId);

        modelBuilder.Entity<CleanerTask>()
            .HasKey(t => t.CleanerTaskId);

        modelBuilder.Entity<Breakage>()
            .HasKey(b => b.BreakageId);

        modelBuilder.Entity<Notification>()
            .HasKey(n => n.NotificationId);

        modelBuilder.Entity<Report>()
            .HasKey(r => r.ReportId);

        modelBuilder.Entity<Owner>()
            .HasMany(o => o.Properties)
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Property>()
            .HasMany(p => p.Bookings)
            .WithOne(b => b.Property)
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cleaner>()
            .HasMany(c => c.CleanerTasks)
            .WithOne(t => t.Cleaner)
            .HasForeignKey(t => t.CleanerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Property>()
            .HasMany(p => p.CleanerTasks)
            .WithOne(t => t.Location)
            .HasForeignKey(t => t.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Booking>()
            .HasMany(b => b.Breakages)
            .WithOne(b => b.Booking)
            .HasForeignKey(b => b.BookingId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Property>()
            .HasMany<Breakage>()
            .WithOne(b => b.Location)
            .HasForeignKey(b => b.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Property>()
            .Property(p => p.RatePerNight)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.Rate)
            .HasPrecision(18, 2);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(255);

        modelBuilder.Entity<Owner>()
            .Property(o => o.Email)
            .HasMaxLength(255);

        modelBuilder.Entity<Cleaner>()
            .Property(c => c.Email)
            .HasMaxLength(255);
    }
}