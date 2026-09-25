using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(x => x.Email)
                .IsUnique();

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasIndex(x => x.PhoneNumber)
                .IsUnique();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.LastLoginAt);

            builder.HasOne(x => x.Doctor)
                .WithOne(x => x.User)
                .HasForeignKey<Doctor>(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}