using Clinexa.Enums;

namespace Clinexa.Models.ViewModels.Payment
{
    public class PaymentFilterViewModel
    {
        public string? Search { get; set; }

        public int? InvoiceId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public DateTime? PaymentDateFrom { get; set; }

        public DateTime? PaymentDateTo { get; set; }

        public decimal? MinAmount { get; set; }

        public decimal? MaxAmount { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalCount { get; set; }

        public List<Entities.Payment> Payments { get; set; } = new();
    }
}
