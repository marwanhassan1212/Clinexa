using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Invoice
{
    public class InvoiceEditViewModel
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal SubTotal { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Tax { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; }
    }
}
