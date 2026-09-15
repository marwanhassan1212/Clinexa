using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(x => x.AppointmentId);
            builder.Property(x => x.AppointmentDate).IsRequired();
            builder.Property(x => x.StartTime).IsRequired();
            builder.Property(x => x.EndTime).IsRequired();
            builder.Property(x => x.AppointmentStatus).HasConversion<string>().IsRequired().HasMaxLength(30);
            builder.Property(x => x.Reason).HasMaxLength(200);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.CancellationReason).HasMaxLength(255);
            builder.HasCheckConstraint("CK_Appointments_EndTime_After_StartTime","[EndTime] > [StartTime]");
            builder.HasIndex(x => new
            {
                x.AppointmentDate,
                x.DoctorId,
                x.StartTime
            }).IsUnique();
            builder.HasIndex(x => new
            {
                x.AppointmentDate,
                x.PatientId,
                x.StartTime
            }).IsUnique();
            builder.HasOne(a => a.Patient)
               .WithMany(p => p.Appointments)
               .HasForeignKey(a => a.PatientId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.MedicalRecord)
                .WithOne(m => m.Appointment)
                .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Invoice)
                .WithOne(i => i.Appointment)
                .HasForeignKey<Invoice>(i => i.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
