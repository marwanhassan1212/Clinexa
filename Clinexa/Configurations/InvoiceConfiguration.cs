using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");
            builder.HasKey(x => x.InvoiceId);
            builder.Property(x => x.InvoiceDate).IsRequired();
            builder.Property(x => x.InvoiceStatus).HasConversion<string>().IsRequired().HasMaxLength(40);
            builder.Property(x => x.SubTotal).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Discount).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Tax).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.TotalAmount).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.PaidAmount).HasPrecision(18, 2).IsRequired();
            builder.Property(i => i.RemainingAmount).HasPrecision(18, 2).IsRequired();
            builder.HasCheckConstraint("CK_Invoices_Amounts_NonNegative",
               "[SubTotal] >= 0 AND [Discount] >= 0 AND [Tax] >= 0 " +
               "AND [TotalAmount] >= 0 AND [PaidAmount] >= 0 " +
               "AND [RemainingAmount] >= 0");

            builder.HasCheckConstraint(
           "CK_Invoices_PaidAmount_NotGreaterThanTotal",
           "[PaidAmount] <= [TotalAmount]");

            builder.HasCheckConstraint(
            "CK_Invoices_RemainingAmount_Valid",
            "[RemainingAmount] = [TotalAmount] - [PaidAmount]");

            builder.HasIndex(i => i.PatientId);
            builder.HasIndex(i => i.AppointmentId)
                .IsUnique();

            builder.HasOne(i => i.Patient)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
