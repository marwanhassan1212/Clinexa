namespace Clinexa.Models.Entities
{
    public class InvoiceItem
    {
        public int InvoiceItemId { get; set; }
        public string Description { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public int InvoiceId { get; set; }

    }
}
