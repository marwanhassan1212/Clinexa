using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.PrescriptionItem;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    public class PrescriptionItemController : Controller
    {
        private readonly IPrescriptionItemService prescriptionItemService;
        private readonly IMedicineService medicineService;

        public PrescriptionItemController(
            IPrescriptionItemService prescriptionItemService,
            IMedicineService medicineService)
        {
            this.prescriptionItemService = prescriptionItemService;
            this.medicineService = medicineService;
        }

        private async Task LoadMedicinesAsync(
            PrescriptionItemCreateViewModel model)
        {
            model.Medicines =
                await medicineService.GetActiveAsync();
        }

        private async Task LoadMedicinesAsync(
            PrescriptionItemEditViewModel model)
        {
            model.Medicines =
                await medicineService.GetActiveAsync();
        }

        // GET: PrescriptionItem
        public async Task<IActionResult> Index()
        {
            var prescriptionItems =
                await prescriptionItemService.GetAllAsync();

            return View(prescriptionItems);
        }

        // GET: PrescriptionItem/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var prescriptionItem =
                await prescriptionItemService.GetByIdAsync(id);

            if (prescriptionItem == null)
                return NotFound();

            return View(prescriptionItem);
        }

        // GET: PrescriptionItem/Create?prescriptionId=5
        [HttpGet]
        public async Task<IActionResult> Create(int prescriptionId)
        {
            var model = new PrescriptionItemCreateViewModel
            {
                PrescriptionId = prescriptionId
            };

            await LoadMedicinesAsync(model);

            return View(model);
        }

        // POST: PrescriptionItem/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PrescriptionItemCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadMedicinesAsync(model);
                return View(model);
            }

            var prescriptionItem = new PrescriptionItem
            {
                PrescriptionId = model.PrescriptionId,
                MedicineId = model.MedicineId,
                Dosage = model.Dosage,
                Frequency = model.Frequency,
                Duration = model.Duration,
                Instructions = model.Instructions
            };

            var result =
                await prescriptionItemService
                    .CreateAsync(prescriptionItem);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create prescription item. Please check the prescription or medicine.");

                await LoadMedicinesAsync(model);
                return View(model);
            }

            TempData["Success"] =
                "Prescription item created successfully.";

            return RedirectToAction(
                "Details",
                "Prescription",
                new { id = prescriptionItem.PrescriptionId });
        }

        // GET: PrescriptionItem/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var prescriptionItem =
                await prescriptionItemService.GetByIdAsync(id);

            if (prescriptionItem == null)
                return NotFound();

            var model = new PrescriptionItemEditViewModel
            {
                PrescriptionItemId =
                    prescriptionItem.PrescriptionItemId,

                MedicineId =
                    prescriptionItem.MedicineId,

                Dosage =
                    prescriptionItem.Dosage,

                Frequency =
                    prescriptionItem.Frequency,

                Duration =
                    prescriptionItem.Duration,

                Instructions =
                    prescriptionItem.Instructions
            };

            await LoadMedicinesAsync(model);

            return View(model);
        }

        // POST: PrescriptionItem/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            PrescriptionItemEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadMedicinesAsync(model);
                return View(model);
            }

            var prescriptionItem =
                await prescriptionItemService
                    .GetByIdAsync(model.PrescriptionItemId);

            if (prescriptionItem == null)
                return NotFound();

            // PrescriptionId is intentionally NOT taken
            // from the ViewModel.
            // The item must remain inside its original Prescription.
            prescriptionItem.MedicineId =
                model.MedicineId;

            prescriptionItem.Dosage =
                model.Dosage;

            prescriptionItem.Frequency =
                model.Frequency;

            prescriptionItem.Duration =
                model.Duration;

            prescriptionItem.Instructions =
                model.Instructions;

            var result =
                await prescriptionItemService
                    .UpdateAsync(prescriptionItem);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update prescription item. Please check the prescription or medicine.");

                await LoadMedicinesAsync(model);
                return View(model);
            }

            TempData["Success"] =
                "Prescription item updated successfully.";

            return RedirectToAction(
                "Details",
                "Prescription",
                new { id = prescriptionItem.PrescriptionId });
        }
    }
}