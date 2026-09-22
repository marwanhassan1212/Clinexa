using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.InvoiceItem
{
    public class InvoiceItemCreateViewModel
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }
    }
}
