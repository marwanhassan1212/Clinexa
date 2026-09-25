using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Payment;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "BillingAccess")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService paymentService;
        private readonly IInvoiceService invoiceService;

        public PaymentController(IPaymentService paymentService , IInvoiceService invoiceService)
        {
            this.paymentService = paymentService;
            this.invoiceService = invoiceService;
        }

        // GET: Payment
        public async Task<IActionResult> Index(PaymentFilterViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            if (model.PageSize <= 0)
            {
                model.PageSize = 10;
            }

            var result = await paymentService.FilterAsync(
                model.Search,
                model.InvoiceId,
                model.PaymentMethod,
                model.PaymentDateFrom,
                model.PaymentDateTo,
                model.MinAmount,
                model.MaxAmount,
                model.Page,
                model.PageSize);

            model.Payments = result.Payments;
            model.TotalCount = result.TotalCount;

            return View(model);
        }

        // GET: Payment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await paymentService.GetByIdAsync(id);

            if (payment == null)
                return NotFound();

            return View(payment);
        }

        // GET: Payment/Create
        [HttpGet]
        public async Task<IActionResult> Create(int invoiceId)
        {
            var invoice = await invoiceService.GetByIdAsync(invoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            var model = new PaymentCreateViewModel
            {
                InvoiceId = invoiceId,
                PaymentDate = DateTime.Today
            };

            return View(model);
        }

        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payment = new Payment
            {
                InvoiceId = model.InvoiceId,
                Amount = model.Amount,
                PaymentDate = model.PaymentDate,
                PaymentMethod = model.PaymentMethod,
                Notes = model.Notes,
                ReferenceNumber = model.ReferenceNumber
            };

            var result = await paymentService.CreateAsync(payment);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create payment. Please check the invoice and payment amount."
                );

                return View(model);
            }

            TempData["Success"] = "Payment created successfully.";

            return RedirectToAction(
                "Details",
                "Invoice",
                new { id = model.InvoiceId });
        }

        // GET: Payment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await paymentService.GetByIdAsync(id);

            if (payment == null)
                return NotFound();

            var model = new PaymentEditViewModel
            {
                PaymentId = payment.PaymentId,
                InvoiceId = payment.InvoiceId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                PaymentMethod = payment.PaymentMethod,
                Notes = payment.Notes,
                ReferenceNumber = payment.ReferenceNumber
            };

            return View(model);
        }

        // POST: Payment/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            PaymentEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payment =
                await paymentService.GetByIdAsync(model.PaymentId);

            if (payment == null)
                return NotFound();

            payment.Amount = model.Amount;
            payment.PaymentDate = model.PaymentDate;
            payment.PaymentMethod = model.PaymentMethod;
            payment.Notes = model.Notes;
            payment.ReferenceNumber = model.ReferenceNumber;

            var result =
                await paymentService.UpdateAsync(payment);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update payment. Please check the invoice and payment amount.");

                return View(model);
            }

            TempData["Success"] =
                "Payment updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Payment/ByInvoice/5
        public async Task<IActionResult> ByInvoice(int invoiceId)
        {
            var payments =
                await paymentService.GetByInvoiceIdAsync(invoiceId);

            return View(payments);
        }
    }
}
