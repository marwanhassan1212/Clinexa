using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Invoice
{
    public class InvoiceCreateViewModel
    {
        public int AppointmentId { get; set; }

        public int PatientId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }
    }
}
