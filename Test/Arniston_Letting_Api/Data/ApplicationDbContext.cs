using Arniston_Letting_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(254);

        modelBuilder.Entity<User>()
            .Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<User>()
            .Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Owner>()
            .Property(o => o.FullName)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Owner>()
            .Property(o => o.Email)
            .IsRequired()
            .HasMaxLength(254);

        modelBuilder.Entity<Owner>()
            .Property(o => o.PhoneNumber)
            .IsRequired()
            .HasMaxLength(30);

        modelBuilder.Entity<Owner>()
            .Property(o => o.AlternativeNumber)
            .HasMaxLength(30);

        modelBuilder.Entity<Owner>()
            .Property(o => o.PreferredContactMethod)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Owner>()
            .Property(o => o.Notes)
            .HasMaxLength(2000);

        modelBuilder.Entity<Property>()
            .Property(p => p.PropertyName)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Property>()
            .Property(p => p.Address)
            .IsRequired()
            .HasMaxLength(500);

        modelBuilder.Entity<Property>()
            .Property(p => p.Description)
            .HasMaxLength(5000);

        modelBuilder.Entity<Property>()
            .Property(p => p.RatePerNight)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .Property(b => b.BookerName)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Booking>()
            .Property(b => b.Rate)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Booking>()
            .HasIndex(b => new
            {
                b.PropertyId,
                b.CheckIn,
                b.CheckOut
            });

        modelBuilder.Entity<Cleaner>()
            .Property(c => c.FullName)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Cleaner>()
            .Property(c => c.PhoneNumber)
            .IsRequired()
            .HasMaxLength(30);

        modelBuilder.Entity<Cleaner>()
            .Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(254);

        modelBuilder.Entity<CleanerTask>()
            .Property(t => t.Notes)
            .HasMaxLength(2000);

        modelBuilder.Entity<Breakage>()
            .Property(b => b.ReportedBy)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Breakage>()
            .Property(b => b.Notes)
            .HasMaxLength(2000);

        modelBuilder.Entity<Notification>()
            .Property(n => n.Type)
            .IsRequired()
            .HasMaxLength(50);

        modelBuilder.Entity<Notification>()
            .Property(n => n.Recipient)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Notification>()
            .Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);

        modelBuilder.Entity<Notification>()
            .Property(n => n.Status)
            .IsRequired()
            .HasMaxLength(30);

        modelBuilder.Entity<Report>()
            .Property(r => r.ReportType)
            .IsRequired()
            .HasMaxLength(100);

        modelBuilder.Entity<Report>()
            .Property(r => r.GeneratedBy)
            .IsRequired()
            .HasMaxLength(150);

        modelBuilder.Entity<Report>()
            .Property(r => r.Status)
            .IsRequired()
            .HasMaxLength(30);

        modelBuilder.Entity<Report>()
            .Property(r => r.Description)
            .HasMaxLength(5000);

        modelBuilder.Entity<Owner>()
            .HasMany(o => o.Properties)
            .WithOne(p => p.Owner)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Property>()
            .HasMany(p => p.Bookings)
            .WithOne(b => b.Property)
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

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
    }
}