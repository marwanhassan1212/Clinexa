using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
    {
        public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
        {
            builder.ToTable("DoctorSchedules");
            builder.HasKey(x => x.DoctorScheduleId);
            builder.Property(x => x.DayOfWeek).HasConversion<string>().IsRequired().HasMaxLength(30);
            builder.Property(x => x.StartTime).IsRequired();
            builder.Property(x => x.EndTime).IsRequired();
            builder.HasIndex(x => new
            {
                x.DoctorId,
                x.DayOfWeek,
                x.StartTime
            }).IsUnique();
            builder.HasCheckConstraint("CK_DoctorSchedules_EndTime_After_StartTime", "[EndTime] > [StartTime]");
            builder.HasOne(ds => ds.Doctor).WithMany(d => d.DoctorSchedules).HasForeignKey(ds => ds.DoctorId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
