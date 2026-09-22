using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Invoice
{
    public class InvoiceEditViewModel
    {
        public int InvoiceId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Discount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Tax { get; set; }
    }
}
