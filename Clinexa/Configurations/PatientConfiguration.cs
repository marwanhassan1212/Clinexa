using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasKey(x => x.PatientId);
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(30);
            builder.HasIndex(x => x.PhoneNumber).IsUnique();
            builder.Property(x => x.Email).HasMaxLength(200);
            builder.HasIndex(x => x.Email);
            builder.Property(x => x.Address).HasMaxLength(500);
            builder.Property(x => x.EmergencyContactName).HasMaxLength(255);
            builder.Property(x => x.EmergencyContactPhone).HasMaxLength(30);
            builder.Property(x => x.Allergies).HasMaxLength(2000);
            builder.Property(x => x.BloodType).HasMaxLength(10);
            builder.Property(x => x.Gender).IsRequired().HasConversion<string>().HasMaxLength(50);
            builder.Property(x => x.DateOfBirth).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
        }
    }
}
