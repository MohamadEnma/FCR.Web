using FCR.Dal.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace FCR.Dal.Configuration
{
    public class CarConfiguration : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            builder.ToTable("Cars");

            // Primary Key
            builder.HasKey(c => c.CarId);

            // Properties
            builder.Property(c => c.Brand)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.ModelName)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Category)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.DailyRate)
                   .HasPrecision(10, 2)
                   .IsRequired();

            builder.Property(c => c.Transmission)
                   .HasMaxLength(20)
                   .HasDefaultValue("Automatic");

            builder.Property(c => c.FuelType)
                   .HasMaxLength(20)
                   .HasDefaultValue("Petrol");

            builder.Property(c => c.Seats)
                   .HasDefaultValue(5);

            builder.Property(c => c.IsAvailable)
                   .HasDefaultValue(true);

            builder.Property(c => c.IsDeleted)
                   .HasDefaultValue(false);

            builder.Property(c => c.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Unique Constraint
            builder.HasIndex(c => c.LicensePlate)
                   .IsUnique()
                   .HasFilter("[LicensePlate] IS NOT NULL");

            // Indexes for Performance
            builder.HasIndex(c => c.IsAvailable);
            builder.HasIndex(c => c.IsDeleted);
            builder.HasIndex(c => c.Category);
            builder.HasIndex(c => c.Brand);
            builder.HasIndex(c => new { c.IsAvailable, c.IsDeleted }); // Composite index

            // Relationships
            builder.HasMany(c => c.Images)
                   .WithOne(i => i.Car)
                   .HasForeignKey(i => i.CarId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Bookings)
                   .WithOne(b => b.Car)
                   .HasForeignKey(b => b.CarId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}