using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Invoice;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            this.invoiceService = invoiceService;
        }

        // GET: Invoice
        public async Task<IActionResult> Index()
        {
            var invoices =
                await invoiceService.GetAllAsync();

            return View(invoices);
        }

        
        public async Task<IActionResult> Details(int id)
        {
            var invoice =
                await invoiceService.GetByIdAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Invoice/Create
        public IActionResult Create()
        {
            return View();
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

            var invoice = new Invoice
            {
                InvoiceDate = model.InvoiceDate,
                PatientId = model.PatientId,
                AppointmentId = model.AppointmentId,
                SubTotal = model.SubTotal,
                Discount = model.Discount,
                Tax = model.Tax,
                PaidAmount = model.PaidAmount
            };

            var result =
                await invoiceService
                    .CreateAsync(invoice);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create invoice. Please check the patient, appointment, or invoice amounts."
                );

                return View(model);
            }

            TempData["Success"] =
                "Invoice created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Invoice/Edit/5
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
                InvoiceId =
                    invoice.InvoiceId,

                InvoiceDate =
                    invoice.InvoiceDate,

                PatientId =
                    invoice.PatientId,

                AppointmentId =
                    invoice.AppointmentId,

                SubTotal =
                    invoice.SubTotal,

                Discount =
                    invoice.Discount,

                Tax =
                    invoice.Tax,

                PaidAmount =
                    invoice.PaidAmount
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

            invoice.PatientId =
                model.PatientId;

            invoice.AppointmentId =
                model.AppointmentId;

            invoice.SubTotal =
                model.SubTotal;

            invoice.Discount =
                model.Discount;

            invoice.Tax =
                model.Tax;

            invoice.PaidAmount =
                model.PaidAmount;

            var result =
                await invoiceService
                    .UpdateAsync(invoice);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update invoice. Please check the patient, appointment, or invoice amounts."
                );

                return View(model);
            }

            TempData["Success"] =
                "Invoice updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
