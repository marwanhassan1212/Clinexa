using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.ToTable("Doctors");
            builder.HasKey(x => x.DoctorId);
            builder.Property(d => d.ConsultationFee).HasPrecision(18, 2).IsRequired();
            builder.HasCheckConstraint("CK_Doctors_ConsultationFee_NonNegative", "[ConsultationFee] >= 0");
            builder.Property(d => d.IsActive).IsRequired();
            builder.Property(d => d.CreatedAt).IsRequired();
            builder.HasIndex(d => d.UserId).IsUnique();
            builder.HasOne(d => d.Speciality).WithMany(s => s.Doctors).HasForeignKey(d => d.SpecialityId).OnDelete(DeleteBehavior.Restrict);



        }
    }
}
