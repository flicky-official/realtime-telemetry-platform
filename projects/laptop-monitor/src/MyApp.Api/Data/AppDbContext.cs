using Microsoft.EntityFrameworkCore;
using MyApp.Api.Models;

namespace MyApp.Api.Data;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<TelemetrySample> TelemetrySamples => Set<TelemetrySample>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var devices = modelBuilder.Entity<Device>();

        devices.ToTable("devices");
        devices.HasKey(d => d.Id);
        devices.Property(d => d.Id).HasMaxLength(64);
        devices.Property(d => d.Name).HasMaxLength(120).IsRequired();
        devices.Property(d => d.CreatedAtUtc)
            .HasColumnType("timestamp with time zone");

        var samples = modelBuilder.Entity<TelemetrySample>();

        samples.ToTable("telemetry_samples");
        samples.HasKey(s => s.Id);

        // The agent must supply the message ID.
        samples.Property(s => s.Id).ValueGeneratedNever();

        samples.Property(s => s.DeviceId)
            .HasMaxLength(64)
            .IsRequired();

        samples.Property(s => s.RecordedAtUtc)
            .HasColumnType("timestamp with time zone");

        samples.Property(s => s.ReceivedAtUtc)
            .HasColumnType("timestamp with time zone");

        samples.HasOne<Device>()
            .WithMany()
            .HasForeignKey(s => s.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Supports latest readings and ordered history for one device.
        samples.HasIndex(s => new
        {
            s.DeviceId,
            s.RecordedAtUtc,
            s.Id
        });
    }
}
