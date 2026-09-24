using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository invoiceRepository;
        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            this.invoiceRepository = invoiceRepository;
        }
        public async Task<bool> CreateAsync(Invoice invoice)
        {
            // 1. Patient must exist
            bool patientExists =
                await invoiceRepository
                    .PatientExistsAsync(invoice.PatientId);

            if (!patientExists)
            {
                return false;
            }

            // 2. Appointment must exist
            bool appointmentExists =
                await invoiceRepository
                    .AppointmentExistsAsync(invoice.AppointmentId);

            if (!appointmentExists)
            {
                return false;
            }

            bool appointmentCancelled =
                await invoiceRepository
                    .IsAppointmentCancelledAsync(
                        invoice.AppointmentId);

            if (appointmentCancelled)
            {
                return false;
            }


            bool invoiceExists =
                await invoiceRepository
                    .ExistsForAppointmentAsync(invoice.AppointmentId);

            if (invoiceExists)
            {
                return false;
            }

            // 4. Financial validation
            if (invoice.SubTotal < 0 ||
                invoice.Discount < 0 ||
                invoice.Tax < 0)
            {
                return false;
            }

            // 5. Discount cannot exceed subtotal
            if (invoice.Discount > invoice.SubTotal)
            {
                return false;
            }

            // 6. Calculate total amount
            invoice.TotalAmount =
                invoice.SubTotal
                - invoice.Discount
                + invoice.Tax;

            // 7. Paid amount must be valid
            if (invoice.PaidAmount < 0 ||
                invoice.PaidAmount > invoice.TotalAmount)
            {
                return false;
            }

            // 8. Calculate remaining amount
            invoice.RemainingAmount =
                invoice.TotalAmount
                - invoice.PaidAmount;

            // 9. Determine invoice status
            if (invoice.PaidAmount == 0)
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.Unpaid;
            }
            else if (invoice.PaidAmount < invoice.TotalAmount)
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.Paid;
            }

            // 10. Set invoice date
            if (invoice.InvoiceDate == default)
            {
                invoice.InvoiceDate = DateTime.Now;
            }

            await invoiceRepository.AddAsync(invoice);

            await invoiceRepository.SaveChangesAsync();

            return true;
        }

        public async Task<(List<Invoice> Invoices, int TotalCount)> FilterAsync(
                   string? search,
                   int? patientId,
                   string? invoiceStatus,
                   DateTime? dateFrom,
                   DateTime? dateTo,
                   string sortBy,
                   string sortDirection,
                   int page,
                   int pageSize)
        {
            return await invoiceRepository.FilterAsync(
                        search,
                        patientId,
                        invoiceStatus,
                        dateFrom,
                        dateTo,
                        sortBy,
                        sortDirection,
                        page,
                        pageSize);
        }

        public async Task<List<Invoice>> GetAllAsync()
        {
            return await invoiceRepository.GetAllAsync();
        }

        public async Task<Invoice?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await invoiceRepository.GetByAppointmentIdAsync(appointmentId);
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await invoiceRepository.GetByIdAsync(id);
        }

        public async Task<List<Invoice>> GetByPatientIdAsync(int patientId)
        {
            return await invoiceRepository.GetByPatientIdAsync(patientId);
        }

        public async Task<bool> UpdateAsync(Invoice invoice)
        {
            // 1. Invoice must exist
            var existingInvoice =
                await invoiceRepository
                    .GetByIdAsync(invoice.InvoiceId);

            if (existingInvoice == null)
            {
                return false;
            }

            // 2. Patient must exist
            bool patientExists =
                await invoiceRepository
                    .PatientExistsAsync(invoice.PatientId);

            if (!patientExists)
            {
                return false;
            }

            // 3. Appointment must exist
            bool appointmentExists =
                await invoiceRepository
                    .AppointmentExistsAsync(invoice.AppointmentId);

            if (!appointmentExists)
            {
                return false;
            }

            // 4. Appointment can only have one invoice
            bool invoiceExists =
                await invoiceRepository
                    .ExistsForAppointmentAsync(
                        invoice.AppointmentId,
                        invoice.InvoiceId);

            if (invoiceExists)
            {
                return false;
            }

            // 5. Financial validation
            if (invoice.SubTotal < 0 ||
                invoice.Discount < 0 ||
                invoice.Tax < 0)
            {
                return false;
            }

            if (invoice.Discount > invoice.SubTotal)
            {
                return false;
            }

            // 6. Recalculate total
            invoice.TotalAmount =
                invoice.SubTotal
                - invoice.Discount
                + invoice.Tax;

            if (existingInvoice.PaidAmount < 0 ||
            existingInvoice.PaidAmount > invoice.TotalAmount)
            {
                return false;
            }

            invoice.PaidAmount =
                existingInvoice.PaidAmount;

            invoice.RemainingAmount =
                invoice.TotalAmount
                - invoice.PaidAmount;

            if (invoice.PaidAmount == 0)
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.Unpaid;
            }
            else if (invoice.PaidAmount < invoice.TotalAmount)
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.InvoiceStatus =
                    Enums.InvoiceStatus.Paid;
            }

            invoiceRepository.Update(invoice);

            await invoiceRepository.SaveChangesAsync();

            return true;
        }
    }
}
