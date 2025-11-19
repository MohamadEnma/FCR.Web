using FCR.Dal.Classes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FCR.Dal.Configuration
{
    public class ImageConfiguration : IEntityTypeConfiguration<Image>
    {
        public void Configure(EntityTypeBuilder<Image> builder)
        {
            builder.ToTable("Images");

            builder.Property(i => i.Url)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(i => i.AltText)
                   .HasMaxLength(150);

            builder.Property(i => i.IsPrimary)
                   .HasDefaultValue(false);

            builder.Property(i => i.DisplayOrder)
                   .HasDefaultValue(0);


            builder.HasOne(i => i.Car)
                   .WithMany(c => c.Images)
                   .HasForeignKey(i => i.CarId)
                   .OnDelete(DeleteBehavior.Cascade);

           builder.HasIndex(i => i.CarId);
        }
    }
}