using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
    {
        public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
        {
            builder.ToTable("PrescriptionItems");
            builder.HasKey(x => x.PrescriptionItemId);
            builder.Property(x => x.Instructions).HasMaxLength(500);
            builder.Property(x => x.Dosage).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Frequency).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Duration).IsRequired().HasMaxLength(300);
            builder.HasIndex(x => new
            {
                x.MedicineId,
                x.PrescriptionId
            }).IsUnique();

            builder.HasOne(pi => pi.Prescription)
           .WithMany(p => p.PrescriptionItems)
           .HasForeignKey(pi => pi.PrescriptionId)
           .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pi => pi.Medicine)
                .WithMany(m => m.PrescriptionItems)
                .HasForeignKey(pi => pi.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
