using Clinexa.Enums;
using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Payment
{
    public class PaymentEditViewModel
    {
        [Required]
        public int PaymentId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(300)]
        public string? ReferenceNumber { get; set; }
    }
}
