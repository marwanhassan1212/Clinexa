using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.ToTable("MedicalRecords");
            builder.HasKey(x => x.MedicalRecordId);
            builder.Property(x => x.Symptoms).HasMaxLength(1000);
            builder.Property(x => x.Diagnosis).HasMaxLength(1000);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.Treatment).HasMaxLength(2000);
            builder.Property(m => m.CreatedAt).IsRequired();
            builder.HasIndex(m => m.PatientId);
            builder.HasIndex(m => m.DoctorId);
            builder.HasIndex(m => m.AppointmentId).IsUnique();

            builder.HasOne(m => m.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(m => m.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Doctor)
               .WithMany(d => d.MedicalRecords)
               .HasForeignKey(m => m.DoctorId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Prescription)
               .WithOne(p => p.MedicalRecord)
               .HasForeignKey<Prescription>(p => p.MedicalRecordId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Appointment)
                .WithOne(a => a.MedicalRecord)
                .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
