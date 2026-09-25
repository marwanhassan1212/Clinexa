using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Prescription;
using Clinexa.Services.Interfaces;
using Clinexa.Services.PDF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "ClinicalAccess")]
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService prescriptionService;
        private readonly IPrescriptionItemService prescriptionItemService;
        private readonly IPrescriptionPdfService prescriptionPdfService;
        private readonly UserManager<User> userManager;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            IPrescriptionItemService prescriptionItemService,
            IPrescriptionPdfService prescriptionPdfService,
            UserManager<User> userManager)
        {
            this.prescriptionService = prescriptionService;
            this.prescriptionItemService = prescriptionItemService;
            this.prescriptionPdfService = prescriptionPdfService;
            this.userManager = userManager;
        }

        // =========================================================
        // Index
        // =========================================================

        public async Task<IActionResult> Index(
            PrescriptionFilterViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            model.PageSize = 10;

            var currentUserIdString =
                userManager.GetUserId(User);

            if (!int.TryParse(
                    currentUserIdString,
                    out var currentUserId))
            {
                return Forbid();
            }

            var currentUser =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
            {
                return Forbid();
            }

            int? doctorUserId = null;

            // Admin → null → can see all prescriptions.
            // Doctor → current user ID → only own prescriptions.
            if (await userManager.IsInRoleAsync(
                    currentUser,
                    "Doctor"))
            {
                doctorUserId = currentUserId;
            }

            var result =
                await prescriptionService
                    .FilterAsync(
                        model.Search,
                        model.DateFrom,
                        model.DateTo,
                        model.MedicalRecordId,
                        model.SortBy,
                        model.SortDirection,
                        model.Page,
                        model.PageSize,
                        doctorUserId);

            model.Prescriptions =
                result.Prescriptions;

            model.TotalPages =
                (int)Math.Ceiling(
                    result.TotalCount /
                    (double)model.PageSize);

            model.MedicalRecords =
                await prescriptionService
                    .GetMedicalRecordsAsync(
                        doctorUserId);

            return View(model);
        }

        // =========================================================
        // Details
        // =========================================================

        public async Task<IActionResult> Details(int id)
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
                await prescriptionService
                    .CanAccessAsync(
                        id,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescription =
                await prescriptionService
                    .GetByIdAsync(id);

            if (prescription == null)
            {
                return NotFound();
            }

            var prescriptionItems =
                await prescriptionItemService
                    .GetByPrescriptionIdAsync(id);

            ViewBag.PrescriptionItems =
                prescriptionItems;

            return View(prescription);
        }

        // =========================================================
        // Create - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create(
            int? medicalRecordId)
        {
            if (!medicalRecordId.HasValue)
            {
                return BadRequest();
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
                await prescriptionService
                    .CanAccessMedicalRecordAsync(
                        medicalRecordId.Value,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var existingPrescription =
                await prescriptionService
                    .GetByMedicalRecordIdAsync(
                        medicalRecordId.Value);

            if (existingPrescription != null)
            {
                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = existingPrescription.PrescriptionId
                    });
            }

            var model =
                new PrescriptionCreateViewModel
                {
                    MedicalRecordId =
                        medicalRecordId.Value,

                    PrescriptionDate =
                        DateTime.Today
                };

            return View(model);
        }

        // =========================================================
        // Create - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PrescriptionCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
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
                await prescriptionService
                    .CanAccessMedicalRecordAsync(
                        model.MedicalRecordId,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescription =
                new Prescription
                {
                    PrescriptionDate =
                        model.PrescriptionDate,

                    Notes =
                        model.Notes,

                    MedicalRecordId =
                        model.MedicalRecordId
                };

            var result =
                await prescriptionService
                    .CreateAsync(
                        prescription);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create prescription. " +
                    "Please check the medical record, prescription date, " +
                    "or existing prescription.");

                return View(model);
            }

            TempData["Success"] =
                "Prescription created successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = prescription.PrescriptionId
                });
        }

        // =========================================================
        // Edit - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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
                await prescriptionService
                    .CanAccessAsync(
                        id,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescription =
                await prescriptionService
                    .GetByIdAsync(id);

            if (prescription == null)
            {
                return NotFound();
            }

            var model =
                new PrescriptionEditViewModel
                {
                    PrescriptionId =
                        prescription.PrescriptionId,

                    PrescriptionDate =
                        prescription.PrescriptionDate,

                    Notes =
                        prescription.Notes
                };

            return View(model);
        }

        // =========================================================
        // Edit - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            PrescriptionEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
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
                await prescriptionService
                    .CanAccessAsync(
                        model.PrescriptionId,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescription =
                await prescriptionService
                    .GetByIdAsync(
                        model.PrescriptionId);

            if (prescription == null)
            {
                return NotFound();
            }

            prescription.PrescriptionDate =
                model.PrescriptionDate;

            prescription.Notes =
                model.Notes;

            var result =
                await prescriptionService
                    .UpdateAsync(
                        prescription);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update prescription. " +
                    "Please check the prescription date.");

                return View(model);
            }

            TempData["Success"] =
                "Prescription updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = prescription.PrescriptionId
                });
        }

        // =========================================================
        // Search Medical Records
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> SearchMedicalRecords(
            string? search)
        {
            var currentUserIdString =
                userManager.GetUserId(User);

            if (!int.TryParse(
                    currentUserIdString,
                    out var currentUserId))
            {
                return Forbid();
            }

            var currentUser =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
            {
                return Forbid();
            }

            int? doctorUserId = null;

            // Admin → null → search all medical records.
            // Doctor → current user ID → search only own records.
            if (await userManager.IsInRoleAsync(
                    currentUser,
                    "Doctor"))
            {
                doctorUserId = currentUserId;
            }

            var records =
                await prescriptionService
                    .SearchMedicalRecordsAsync(
                        search,
                        10,
                        doctorUserId);

            var result =
                records.Select(x => new
                {
                    id = x.MedicalRecordId,

                    text =
                        $"#{x.MedicalRecordId} — " +
                        $"{x.Patient.FirstName} {x.Patient.LastName} — " +
                        $"Dr. {x.Doctor.User.FirstName} " +
                        $"{x.Doctor.User.LastName} — " +
                        $"{x.Appointment.AppointmentDate:dd MMM yyyy}"
                });

            return Json(result);
        }

        // =========================================================
        // Print
        // =========================================================

        public async Task<IActionResult> Print(int id)
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
                await prescriptionService
                    .CanAccessAsync(
                        id,
                        currentUserId);

            if (!canAccess)
            {
                return Forbid();
            }

            var prescription =
                await prescriptionService
                    .GetByIdAsync(id);

            if (prescription == null)
            {
                return NotFound();
            }

            var pdf =
                prescriptionPdfService
                    .Generate(prescription);

            return File(
                pdf,
                "application/pdf",
                $"Prescription-{prescription.PrescriptionId}.pdf");
        }
    }
}