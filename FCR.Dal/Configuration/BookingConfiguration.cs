using FCR.Dal.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace FCR.Dal.Configuration
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("Bookings");

            builder.Property(b => b.Status)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasDefaultValue("Pending");

            builder.Property(b => b.IsCancelled)
                   .HasDefaultValue(false);

            builder.Property(b => b.TotalPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(b => b.BookingNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(b => b.CancellationReason)
                   .HasMaxLength(500);

            builder.Property(b => b.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(b => b.UpdatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(b => b.Car)
                   .WithMany(c => c.Bookings)
                   .HasForeignKey(b => b.CarId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.User)
                   .WithMany(u => u.Bookings)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(b => b.UserId);
            builder.HasIndex(b => b.CarId);
            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => b.PickupDate);
            builder.HasIndex(b => b.BookingNumber).IsUnique();
        }
    }
}