using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Invoice;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService invoiceService;
        private readonly IAppointmentService appointmentService;
        private readonly IInvoiceItemService invoiceItemService;
        private readonly IPaymentService paymentService;

        public InvoiceController(
            IInvoiceService invoiceService,
            IAppointmentService appointmentService,
            IInvoiceItemService invoiceItemService
            ,IPaymentService paymentService)
        {
            this.invoiceService = invoiceService;
            this.appointmentService = appointmentService;
            this.invoiceItemService = invoiceItemService;
            this.paymentService = paymentService;
        }

        // GET: Invoice
        public async Task<IActionResult> Index(
            string? search,
            int? patientId,
            InvoiceStatus? invoiceStatus,
            DateTime? dateFrom,
            DateTime? dateTo,
            string sortBy = "Date",
            string sortDirection = "Desc",
            int page = 1)
        {
            var result =
                await invoiceService.FilterAsync(
                    search,
                    patientId,
                    invoiceStatus?.ToString(),
                    dateFrom,
                    dateTo,
                    sortBy,
                    sortDirection,
                    page,
                    10);

            var model = new InvoiceFilterViewModel
            {
                Search = search,
                PatientId = patientId,
                InvoiceStatus = invoiceStatus,
                DateFrom = dateFrom,
                DateTo = dateTo,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Page = page,
                PageSize = 10,
                Invoices = result.Invoices,
                TotalPages =
                    (int)Math.Ceiling(
                        (double)result.TotalCount / 10)
            };

            return View(model);
        }

        // GET: Invoice/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await invoiceService.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            var items = await invoiceItemService.GetByInvoiceIdAsync(id);

            var payments = await paymentService.GetByInvoiceIdAsync(id);

            var model = new InvoiceDetailsViewModel
            {
                Invoice = invoice,
                InvoiceItems = items,
                Payments = payments
            };

            return View(model);
        }

        // GET: Invoice/Create?appointmentId=5
        [HttpGet]
        public async Task<IActionResult> Create(int appointmentId)
        {
            var appointment =
                await appointmentService
                    .GetByIdAsync(appointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            var existingInvoice =
                await invoiceService
                    .GetByAppointmentIdAsync(appointmentId);

            if (existingInvoice != null)
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id = existingInvoice.InvoiceId });
            }

            var model = new InvoiceCreateViewModel
            {
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                InvoiceDate = DateTime.Today
            };

            return View(model);
        }

        // POST: Invoice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            InvoiceCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var appointment =
                await appointmentService
                    .GetByIdAsync(model.AppointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            var existingInvoice =
                await invoiceService
                    .GetByAppointmentIdAsync(
                        model.AppointmentId);

            if (existingInvoice != null)
            {
                return RedirectToAction(
                    nameof(Details),
                    new { id = existingInvoice.InvoiceId });
            }

            var invoice = new Invoice
            {
                InvoiceDate = model.InvoiceDate,
                AppointmentId = appointment.AppointmentId,
                PatientId = appointment.PatientId,
                SubTotal = 0,
                Discount = 0,
                Tax = 0,
                PaidAmount = 0
            };

            var result =
                await invoiceService
                    .CreateAsync(invoice);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create invoice.");

                return View(model);
            }

            TempData["Success"] =
                "Invoice created successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = invoice.InvoiceId });
        }

        // GET: Invoice/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var invoice =
                await invoiceService
                    .GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            var model = new InvoiceEditViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceDate = invoice.InvoiceDate,
                Discount = invoice.Discount,
                Tax = invoice.Tax
            };

            return View(model);
        }

        // POST: Invoice/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            InvoiceEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var invoice =
                await invoiceService
                    .GetByIdAsync(model.InvoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            invoice.InvoiceDate =
                model.InvoiceDate;

            invoice.Discount =
                model.Discount;

            invoice.Tax =
                model.Tax;

            var result =
                await invoiceService
                    .UpdateAsync(invoice);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update invoice. Please check the invoice date, discount, or tax.");

                return View(model);
            }

            TempData["Success"] =
                "Invoice updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = invoice.InvoiceId });
        }
    }
}