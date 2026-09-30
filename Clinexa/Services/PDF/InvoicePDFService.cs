using Clinexa.Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Clinexa.Services.PDF
{
    public class InvoicePdfService : IInvoicePdfService
    {
        public byte[] Generate(
            Invoice invoice,
            IEnumerable<InvoiceItem> invoiceItems,
            IEnumerable<Payment> payments)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10));

                    // =====================================================
                    // Header
                    // =====================================================

                    page.Header()
                        .Text("CLINEXA")
                        .FontSize(22)
                        .Bold();

                    // =====================================================
                    // Content
                    // =====================================================

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(15);

                            // Title
                            column.Item()
                                .Text("INVOICE")
                                .FontSize(18)
                                .Bold();

                            column.Item()
                                .Text(
                                    $"Invoice #{invoice.InvoiceId}");

                            column.Item()
                                .Text(
                                    $"Date: {invoice.InvoiceDate:dd MMM yyyy}");

                            column.Item()
                                .LineHorizontal(1);

                            // =================================================
                            // Patient Information
                            // =================================================

                            column.Item()
                                .Text("Patient Information")
                                .FontSize(13)
                                .Bold();

                            column.Item()
                                .Text(
                                    $"Patient: " +
                                    $"{invoice.Patient.FirstName} " +
                                    $"{invoice.Patient.LastName}");

                            if (invoice.Appointment != null &&
                                invoice.Appointment.Doctor != null &&
                                invoice.Appointment.Doctor.User != null)
                            {
                                column.Item()
                                    .Text(
                                        $"Doctor: Dr. " +
                                        $"{invoice.Appointment.Doctor.User.FirstName} " +
                                        $"{invoice.Appointment.Doctor.User.LastName}");
                            }

                            column.Item()
                                .Text(
                                    $"Appointment: #{invoice.AppointmentId}");

                            column.Item()
                                .LineHorizontal(1);

                            // =================================================
                            // Invoice Items
                            // =================================================

                            column.Item()
                                .Text("Invoice Items")
                                .FontSize(13)
                                .Bold();

                            column.Item()
                                .Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(4);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(2);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell()
                                            .Text("Item")
                                            .Bold();

                                        header.Cell()
                                            .Text("Quantity")
                                            .Bold();

                                        header.Cell()
                                            .Text("Unit Price")
                                            .Bold();

                                        header.Cell()
                                            .Text("Total")
                                            .Bold();
                                    });

                                    foreach (var item in invoiceItems)
                                    {
                                        table.Cell()
                                            .Text(item.Description);

                                        table.Cell()
                                            .Text(
                                                item.Quantity.ToString());

                                        table.Cell()
                                            .Text(
                                                item.UnitPrice.ToString("0.00"));

                                        table.Cell()
                                            .Text(
                                                (item.Quantity *
                                                 item.UnitPrice)
                                                .ToString("0.00"));
                                    }
                                });

                            column.Item()
                                .LineHorizontal(1);

                            // =================================================
                            // Financial Summary
                            // =================================================

                            column.Item()
                                .Text("Payment Summary")
                                .FontSize(13)
                                .Bold();

                            column.Item()
                                .Text(
                                    $"Subtotal: {invoice.SubTotal:0.00}");

                            column.Item()
                                .Text(
                                    $"Discount: {invoice.Discount:0.00}");

                            column.Item()
                                .Text(
                                    $"Tax: {invoice.Tax:0.00}");

                            column.Item()
                                .Text(
                                    $"Total: {invoice.TotalAmount:0.00}")
                                .Bold();

                            column.Item()
                                .Text(
                                    $"Paid: {invoice.PaidAmount:0.00}");

                            column.Item()
                                .Text(
                                    $"Remaining: {invoice.RemainingAmount:0.00}");

                            column.Item()
                                .Text(
                                    $"Status: {invoice.InvoiceStatus}")
                                .Bold();

                            // =================================================
                            // Payments
                            // =================================================

                            if (payments.Any())
                            {
                                column.Item()
                                    .PaddingTop(10)
                                    .Text("Payments")
                                    .FontSize(13)
                                    .Bold();

                                column.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(columns =>
                                        {
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(3);
                                            columns.RelativeColumn(2);
                                        });

                                        table.Header(header =>
                                        {
                                            header.Cell()
                                                .Text("Date")
                                                .Bold();

                                            header.Cell()
                                                .Text("Method")
                                                .Bold();

                                            header.Cell()
                                                .Text("Amount")
                                                .Bold();
                                        });

                                        foreach (var payment in payments)
                                        {
                                            table.Cell()
                                                .Text(
                                                    payment.PaymentDate
                                                        .ToString(
                                                            "dd MMM yyyy"));

                                            table.Cell()
                                                .Text(
                                                    payment.PaymentMethod
                                                        .ToString());

                                            table.Cell()
                                                .Text(
                                                    payment.Amount
                                                        .ToString("0.00"));
                                        }
                                    });
                            }
                        });

                    // =====================================================
                    // Footer
                    // =====================================================

                    page.Footer()
                        .AlignCenter()
                        .Text(
                            "Clinexa • Medical Management System")
                        .FontSize(8);
                });
            });

            return document.GeneratePdf();
        }
    }
}
