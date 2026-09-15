using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Payment;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPaymentService paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }

        // GET: Payment
        public async Task<IActionResult> Index()
        {
            var payments = await paymentService.GetAllAsync();

            return View(payments);
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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PaymentCreateViewModel model)
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
                    "Unable to create payment. Please check the invoice and payment amount.");

                return View(model);
            }

            TempData["Success"] =
                "Payment created successfully.";

            return RedirectToAction(nameof(Index));
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

            payment.InvoiceId = model.InvoiceId;
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
