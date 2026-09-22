using Clinexa.Enums;

namespace Clinexa.Models.ViewModels.Invoice
{
    public class InvoiceFilterViewModel
    {
        public string? Search { get; set; }

        public int? PatientId { get; set; }

        public InvoiceStatus? InvoiceStatus { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public string SortBy { get; set; } = "Date";

        public string SortDirection { get; set; } = "Desc";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        public List<Entities.Invoice> Invoices { get; set; } = new();

        public List<Entities.Patient> Patients { get; set; } = new();
    }
}
