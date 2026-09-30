using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Implementations;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;

namespace Clinexa.Services.Implementations
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository invoiceRepository;

        private readonly IAuditLogService auditLogService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<User> userManager;
        private readonly IDateTimeService dateTimeService;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            IAuditLogService auditLogService,
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IDateTimeService dateTimeService)
        {
            this.invoiceRepository = invoiceRepository;
            this.auditLogService = auditLogService;
            this.httpContextAccessor = httpContextAccessor;
            this.userManager = userManager;
            this.dateTimeService = dateTimeService;
        }


        public async Task<bool> CreateAsync(Invoice invoice)
        {
            bool patientExists =
                await invoiceRepository
                    .PatientExistsAsync(invoice.PatientId);

            if (!patientExists)
                return false;

            bool appointmentExists =
                await invoiceRepository
                    .AppointmentExistsAsync(invoice.AppointmentId);

            if (!appointmentExists)
                return false;

            // Invoice can only be created
            // after the appointment is completed.
            bool appointmentCompleted =
                await invoiceRepository
                    .IsAppointmentCompletedAsync(
                        invoice.AppointmentId);

            if (!appointmentCompleted)
                return false;

            bool invoiceExists =
                await invoiceRepository
                    .ExistsForAppointmentAsync(
                        invoice.AppointmentId);

            if (invoiceExists)
                return false;

            if (invoice.SubTotal < 0 ||
                invoice.Discount < 0 ||
                invoice.Tax < 0)
            {
                return false;
            }

            if (invoice.Discount > invoice.SubTotal)
                return false;

            invoice.TotalAmount =
                invoice.SubTotal
                - invoice.Discount
                + invoice.Tax;

            if (invoice.PaidAmount < 0 ||
                invoice.PaidAmount > invoice.TotalAmount)
            {
                return false;
            }

            invoice.RemainingAmount =
                invoice.TotalAmount
                - invoice.PaidAmount;

            if (invoice.PaidAmount == 0)
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.Unpaid;
            }
            else if (invoice.PaidAmount < invoice.TotalAmount)
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.InvoiceStatus =
                    InvoiceStatus.Paid;
            }

            if (invoice.InvoiceDate == default)
                invoice.InvoiceDate = dateTimeService.Now;

            await invoiceRepository.AddAsync(invoice);

            await invoiceRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Create",
                invoice,
                null);

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

        public async Task<Invoice?> GetByAppointmentIdAsync(
            int appointmentId)
        {
            return await invoiceRepository
                .GetByAppointmentIdAsync(appointmentId);
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await invoiceRepository.GetByIdAsync(id);
        }

        public async Task<List<Invoice>> GetByPatientIdAsync(
            int patientId)
        {
            return await invoiceRepository
                .GetByPatientIdAsync(patientId);
        }

        public async Task<bool> UpdateAsync(Invoice invoice)
        {
            var existingInvoice =
                await invoiceRepository
                    .GetByIdAsync(invoice.InvoiceId);

            if (existingInvoice == null)
                return false;

            bool patientExists =
                await invoiceRepository
                    .PatientExistsAsync(invoice.PatientId);

            if (!patientExists)
                return false;

            bool appointmentExists =
                await invoiceRepository
                    .AppointmentExistsAsync(invoice.AppointmentId);

            if (!appointmentExists)
                return false;

            bool invoiceExists =
                await invoiceRepository
                    .ExistsForAppointmentAsync(
                        invoice.AppointmentId,
                        invoice.InvoiceId);

            if (invoiceExists)
                return false;

            if (invoice.SubTotal < 0 ||
                invoice.Discount < 0 ||
                invoice.Tax < 0)
            {
                return false;
            }

            if (invoice.Discount > invoice.SubTotal)
                return false;

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

            var oldValues = SerializeInvoice(existingInvoice);

            invoiceRepository.Update(invoice);

            await invoiceRepository.SaveChangesAsync();

            await WriteAuditLogAsync(
                "Update",
                invoice,
                oldValues);

            return true;
        }

        private async Task WriteAuditLogAsync(
            string action,
            Invoice invoice,
            string? oldValues)
        {
            var userId = await GetCurrentUserIdAsync();

            if (!userId.HasValue)
                return;

            await auditLogService.LogAsync(
                action: action,
                entityName: nameof(Invoice),
                entityId: invoice.InvoiceId,
                userId: userId.Value,
                oldValues: oldValues,
                newValues: SerializeInvoice(invoice),
                ipAddress: GetIpAddress());
        }

        private async Task<int?> GetCurrentUserIdAsync()
        {
            var userIdClaim =
                httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            var user =
                await userManager.FindByIdAsync(userIdClaim);

            return user?.Id;
        }

        private string? GetIpAddress()
        {
            return httpContextAccessor.HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();
        }

        private string SerializeInvoice(Invoice invoice)
        {
            return JsonSerializer.Serialize(new
            {
                invoice.InvoiceId,
                invoice.PatientId,
                invoice.AppointmentId,
                invoice.SubTotal,
                invoice.Discount,
                invoice.Tax,
                invoice.TotalAmount,
                invoice.PaidAmount,
                invoice.RemainingAmount,
                invoice.InvoiceStatus,
                invoice.InvoiceDate
            });
        }
    }
}