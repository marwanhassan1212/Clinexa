using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Prescription;
using Clinexa.Services.Interfaces;
using Clinexa.Services.PDF;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService prescriptionService;
        private readonly IPrescriptionItemService prescriptionItemService;
        private readonly IPrescriptionPdfService prescriptionPdfService;

        public PrescriptionController(
            IPrescriptionService prescriptionService,
            IPrescriptionItemService prescriptionItemService
            ,IPrescriptionPdfService prescriptionPdfService)
        {
            this.prescriptionService = prescriptionService;
            this.prescriptionItemService = prescriptionItemService;
            this.prescriptionPdfService = prescriptionPdfService;
        }

        public async Task<IActionResult> Index(
            PrescriptionFilterViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            model.PageSize = 10;

            var result =
                await prescriptionService.FilterAsync(
                    model.Search,
                    model.DateFrom,
                    model.DateTo,
                    model.MedicalRecordId,
                    model.SortBy,
                    model.SortDirection,
                    model.Page,
                    model.PageSize);

            model.Prescriptions = result.Prescriptions;

            model.TotalPages =
                (int)Math.Ceiling(
                    result.TotalCount /
                    (double)model.PageSize);

            model.MedicalRecords =
                await prescriptionService
                    .GetMedicalRecordsAsync();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var prescription =
                await prescriptionService.GetByIdAsync(id);

            if (prescription == null)
            {
                return NotFound();
            }

            var prescriptionItems =
                await prescriptionItemService
                    .GetByPrescriptionIdAsync(id);

            ViewBag.PrescriptionItems = prescriptionItems;

            return View(prescription);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? medicalRecordId)
        {
            if (!medicalRecordId.HasValue)
            {
                return BadRequest();
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

            var model = new PrescriptionCreateViewModel
            {
                MedicalRecordId = medicalRecordId.Value,
                PrescriptionDate = DateTime.Today
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PrescriptionCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var prescription = new Prescription
            {
                PrescriptionDate = model.PrescriptionDate,
                Notes = model.Notes,
                MedicalRecordId = model.MedicalRecordId
            };

            var result =
                await prescriptionService
                    .CreateAsync(prescription);

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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var prescription =
                await prescriptionService
                    .GetByIdAsync(id);

            if (prescription == null)
            {
                return NotFound();
            }

            var model = new PrescriptionEditViewModel
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            PrescriptionEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
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
                    .UpdateAsync(prescription);

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

        [HttpGet]
        public async Task<IActionResult> SearchMedicalRecords(
            string? search)
        {
            var records =
                await prescriptionService
                    .SearchMedicalRecordsAsync(
                        search,
                        10);

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

        public async Task<IActionResult> Print(int id)
        {
            var prescription = await prescriptionService.GetByIdAsync(id);

            if (prescription == null)
                return NotFound();

            var pdf = prescriptionPdfService.Generate(prescription);

            return File(
                pdf,
                "application/pdf",
                $"Prescription-{prescription.PrescriptionId}.pdf");
        }
    }
}