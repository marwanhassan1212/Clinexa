namespace Clinexa.Models.ViewModels.Invoice
{
    public class InvoiceDetailsViewModel
    {
        public Entities.Invoice Invoice { get; set; } = null!;

        public List<Entities.InvoiceItem> InvoiceItems { get; set; }
            = new();

        public List<Entities.Payment> Payments { get; set; } = new();
    }
}
