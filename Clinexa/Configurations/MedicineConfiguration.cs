using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
    {
        public void Configure(EntityTypeBuilder<Medicine> builder)
        {
            builder.ToTable("Medicines");
            builder.HasKey(x => x.MedicineId);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.GenericName).HasMaxLength(300);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.HasIndex(x => x.Name);
        }
    }
}
