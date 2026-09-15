using Clinexa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinexa.Configurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.ToTable("InvoiceItems");
            builder.HasKey(x => x.InvoiceItemId);
            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.UnitPrice).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.TotalPrice).HasPrecision(18, 2).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);

            builder.HasCheckConstraint(
               "CK_InvoiceItems_Quantity_Positive",
               "[Quantity] > 0");

            builder.HasCheckConstraint(
                "CK_InvoiceItems_Prices_NonNegative",
                "[UnitPrice] >= 0 AND [TotalPrice] >= 0");

            builder.HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
