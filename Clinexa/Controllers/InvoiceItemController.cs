using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.InvoiceItem;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    public class InvoiceItemController : Controller
    {

        private readonly IInvoiceItemService invoiceItemService;

        public InvoiceItemController(
            IInvoiceItemService invoiceItemService)
        {
            this.invoiceItemService = invoiceItemService;
        }

        // GET: InvoiceItem
        public async Task<IActionResult> Index()
        {
            var invoiceItems =
                await invoiceItemService
                    .GetAllAsync();

            return View(invoiceItems);
        }

        // GET: InvoiceItem/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var invoiceItem =
                await invoiceItemService
                    .GetByIdAsync(id);

            if (invoiceItem == null)
            {
                return NotFound();
            }

            return View(invoiceItem);
        }

        // GET: InvoiceItem/Create
        public IActionResult Create()
        {
            return View();
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

            return RedirectToAction(nameof(Index));
        }

        // GET: InvoiceItem/Edit/5
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

            invoiceItem.InvoiceId =
                model.InvoiceId;

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
                    "Unable to update invoice item. Please check the invoice, quantity, or unit price."
                );

                return View(model);
            }

            TempData["Success"] =
                "Invoice item updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
