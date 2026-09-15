using Clinexa.Enums;

namespace Clinexa.Models.Entities
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public int InvoiceId { get; set; }

    }
}
