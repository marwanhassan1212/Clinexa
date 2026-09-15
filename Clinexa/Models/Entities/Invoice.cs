using Clinexa.Enums;

namespace Clinexa.Models.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public InvoiceStatus InvoiceStatus { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public Patient Patient { get; set; } = null!;
        public int PatientId { get; set; }
        public Appointment Appointment { get; set; } = null!;
        public int AppointmentId { get; set; }
        public ICollection<InvoiceItem> InvoiceItems { get; set; }
           = new List<InvoiceItem>();

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();

    }
}
