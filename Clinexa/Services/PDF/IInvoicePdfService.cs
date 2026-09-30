using Clinexa.Models.Entities;

namespace Clinexa.Services.PDF
{
    public interface IInvoicePdfService
    {
        byte[] Generate(
            Invoice invoice,
            IEnumerable<InvoiceItem> invoiceItems,
            IEnumerable<Payment> payments);
    }
}
