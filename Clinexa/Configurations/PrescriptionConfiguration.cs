using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.ToTable("Prescriptions");
            builder.HasKey(x => x.PrescriptionId);
            builder.Property(x => x.PrescriptionDate).IsRequired();
            builder.Property(x => x.Notes).HasMaxLength(1000);
            builder.HasIndex(x => x.MedicalRecordId).IsUnique();
        }
    }
}
