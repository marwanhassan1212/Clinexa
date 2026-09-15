using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(x => x.AuditLogId);
            builder.Property(x => x.Action).HasMaxLength(300).IsRequired();
            builder.Property(x => x.EntityName).HasMaxLength(300).IsRequired();
            builder.Property(x => x.EntityId).IsRequired();
            builder.Property(a => a.OldValues).HasMaxLength(8000);
            builder.Property(a => a.NewValues).HasMaxLength(8000);
            builder.Property(a => a.Timestamp).IsRequired();
            builder.Property(a => a.IpAddress).HasMaxLength(45);
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.Timestamp);

            builder.HasIndex(a => new
            {
                a.EntityName,
                a.EntityId
            });

            builder.HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
