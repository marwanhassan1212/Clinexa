using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.PrescriptionItem;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "ClinicalAccess")]
    public class PrescriptionItemController : Controller
    {
        private readonly IPrescriptionItemService
            prescriptionItemService;

        private readonly IMedicineService
            medicineService;

        private readonly UserManager<User>
            userManager;

        public PrescriptionItemController(
            IPrescriptionItemService prescriptionItemService,
            IMedicineService medicineService,
            UserManager<User> userManager)
        {
            this.prescriptionItemService =
                prescriptionItemService;

            this.medicineService =
                medicineService;

            this.userManager =
                userManager;
        }

        private async Task LoadMedicinesAsync(
            PrescriptionItemCreateViewModel model)
        {
            model.Medicines =
                await medicineService
                    .GetActiveAsync();
        }

        private async Task LoadMedicinesAsync(
            PrescriptionItemEditViewModel model)
        {
            model.Medicines =
                await medicineService
                    .GetActiveAsync();
        }

        // =========================================================
        // Index
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var prescriptionItems =
                await prescriptionItemService
                    .GetAllAsync();

            return View(prescriptionItems);
        }

        // =========================================================
        // Details
        // =========================================================

        public async Task<IActionResult> Details(
            int id)
        {
            var currentUserIdString =
                userManager.GetUserId(User);

            if (!int.TryParse(
                    currentUserIdString,
                    out var currentUserId))
            {
                return Forbid();
            }

            var canAccess =
                await prescriptionItemService
                    .CanAccessAsync(
                        id,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescriptionItem =
                await prescriptionItemService
                    .GetByIdAsync(id);

            if (prescriptionItem == null)
            {
                return NotFound();
            }

            return View(prescriptionItem);
        }

        // =========================================================
        // Create - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(
            int prescriptionId)
        {
            var currentUserIdString =
                userManager.GetUserId(User);

            if (!int.TryParse(
                    currentUserIdString,
                    out var currentUserId))
            {
                return Forbid();
            }

            var canAccess =
                await prescriptionItemService
                    .CanAccessPrescriptionAsync(
                        prescriptionId,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var model =
                new PrescriptionItemCreateViewModel
                {
                    PrescriptionId =
                        prescriptionId
                };

            await LoadMedicinesAsync(model);

            return View(model);
        }

        // =========================================================
        // Create - POST
        // =========================================================

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

            var currentUserIdString =
                userManager.GetUserId(User);

            if (!int.TryParse(
                    currentUserIdString,
                    out var currentUserId))
            {
                return Forbid();
            }

            var canAccess =
                await prescriptionItemService
                    .CanAccessPrescriptionAsync(
                        model.PrescriptionId,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescriptionItem =
                new PrescriptionItem
                {
                    PrescriptionId =
                        model.PrescriptionId,

                    MedicineId =
                        model.MedicineId,

                    Dosage =
                        model.Dosage,

                    Frequency =
                        model.Frequency,

                    Duration =
                        model.Duration,

                    Instructions =
                        model.Instructions
                };

            var result =
                await prescriptionItemService
                    .CreateAsync(
                        prescriptionItem);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create prescription item. " +
                    "Please check the prescription or medicine.");

                await LoadMedicinesAsync(model);

                return View(model);
            }

            TempData["Success"] =
                "Prescription item created successfully.";

            return RedirectToAction(
                "Details",
                "Prescription",
                new
                {
                    id =
                        prescriptionItem.PrescriptionId
                });
        }

        // =========================================================
        // Edit - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var currentUserIdString =
                userManager.GetUserId(User);

            if (!int.TryParse(
                    currentUserIdString,
                    out var currentUserId))
            {
                return Forbid();
            }

            var canAccess =
                await prescriptionItemService
                    .CanAccessAsync(
                        id,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescriptionItem =
                await prescriptionItemService
                    .GetByIdAsync(id);

            if (prescriptionItem == null)
            {
                return NotFound();
            }

            var model =
                new PrescriptionItemEditViewModel
                {
                    PrescriptionItemId =
                        prescriptionItem
                            .PrescriptionItemId,

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

        // =========================================================
        // Edit - POST
        // =========================================================

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

            var currentUserIdString =
                userManager.GetUserId(User);

            if (!int.TryParse(
                    currentUserIdString,
                    out var currentUserId))
            {
                return Forbid();
            }

            var canAccess =
                await prescriptionItemService
                    .CanAccessAsync(
                        model.PrescriptionItemId,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescriptionItem =
                await prescriptionItemService
                    .GetByIdAsync(
                        model.PrescriptionItemId);

            if (prescriptionItem == null)
            {
                return NotFound();
            }

            // PrescriptionId is intentionally NOT taken
            // from the ViewModel.
            // The item remains inside its original Prescription.

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
                    .UpdateAsync(
                        prescriptionItem);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update prescription item. " +
                    "Please check the prescription or medicine.");

                await LoadMedicinesAsync(model);

                return View(model);
            }

            TempData["Success"] =
                "Prescription item updated successfully.";

            return RedirectToAction(
                "Details",
                "Prescription",
                new
                {
                    id =
                        prescriptionItem.PrescriptionId
                });
        }
    }
}