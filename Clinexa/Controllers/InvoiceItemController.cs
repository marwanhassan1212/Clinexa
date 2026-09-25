using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.InvoiceItem;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "BillingAccess")]
    public class InvoiceItemController : Controller
    {
        private readonly IInvoiceItemService invoiceItemService;

        public InvoiceItemController(
            IInvoiceItemService invoiceItemService)
        {
            this.invoiceItemService = invoiceItemService;
        }

        // GET: InvoiceItem/Create?invoiceId=5
        [HttpGet]
        public async Task<IActionResult> Create(int invoiceId)
        {
            var invoiceExists =
                await invoiceItemService.InvoiceExistsAsync(invoiceId);

            if (!invoiceExists)
            {
                return NotFound();
            }

            var model = new InvoiceItemCreateViewModel
            {
                InvoiceId = invoiceId,
                Quantity = 1
            };

            return View(model);
        }

        // POST: InvoiceItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            InvoiceItemCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var invoiceItem = new InvoiceItem
            {
                InvoiceId = model.InvoiceId,
                Description = model.Description,
                Quantity = model.Quantity,
                UnitPrice = model.UnitPrice
            };

            var result =
                await invoiceItemService
                    .CreateAsync(invoiceItem);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create invoice item. Please check the invoice, quantity, or unit price."
                );

                return View(model);
            }

            TempData["Success"] =
                "Invoice item created successfully.";

            return RedirectToAction(
                "Details",
                "Invoice",
                new { id = model.InvoiceId });
        }

        // GET: InvoiceItem/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var invoiceItem =
                await invoiceItemService
                    .GetByIdAsync(id);

            if (invoiceItem == null)
            {
                return NotFound();
            }

            var model = new InvoiceItemEditViewModel
            {
                InvoiceItemId =
                    invoiceItem.InvoiceItemId,

                InvoiceId =
                    invoiceItem.InvoiceId,

                Description =
                    invoiceItem.Description,

                Quantity =
                    invoiceItem.Quantity,

                UnitPrice =
                    invoiceItem.UnitPrice
            };

            return View(model);
        }

        // POST: InvoiceItem/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            InvoiceItemEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var invoiceItem =
                await invoiceItemService
                    .GetByIdAsync(
                        model.InvoiceItemId);

            if (invoiceItem == null)
            {
                return NotFound();
            }

            // InvoiceId comes from the existing item.
            // The item cannot be moved to another invoice.
            invoiceItem.Description =
                model.Description;

            invoiceItem.Quantity =
                model.Quantity;

            invoiceItem.UnitPrice =
                model.UnitPrice;

            var result =
                await invoiceItemService
                    .UpdateAsync(invoiceItem);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update invoice item. Please check the quantity or unit price."
                );

                return View(model);
            }

            TempData["Success"] =
                "Invoice item updated successfully.";

            return RedirectToAction(
                "Details",
                "Invoice",
                new { id = invoiceItem.InvoiceId });
        }

        // POST: InvoiceItem/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var invoiceItem =
                await invoiceItemService
                    .GetByIdAsync(id);

            if (invoiceItem == null)
            {
                return NotFound();
            }

            var invoiceId =
                invoiceItem.InvoiceId;

            var result =
                await invoiceItemService
                    .DeleteAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "Unable to delete invoice item.";

                return RedirectToAction(
                    "Details",
                    "Invoice",
                    new { id = invoiceId });
            }

            TempData["Success"] =
                "Invoice item deleted successfully.";

            return RedirectToAction(
                "Details",
                "Invoice",
                new { id = invoiceId });
        }
    }
}