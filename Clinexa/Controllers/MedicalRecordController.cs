using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.MedicalRecord;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "ClinicalAccess")]
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordService medicalRecordService;
        private readonly UserManager<User> userManager;

        public MedicalRecordController(
            IMedicalRecordService medicalRecordService,
            UserManager<User> userManager)
        {
            this.medicalRecordService = medicalRecordService;
            this.userManager = userManager;
        }

        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var medicalRecords =
                await medicalRecordService.GetAllAsync();

            return View(medicalRecords);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        public async Task<IActionResult> Details(int id)
        {
            var medicalRecord =
                await medicalRecordService.GetByIdAsync(id);

            if (medicalRecord == null)
                return NotFound();

            return View(medicalRecord);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new MedicalRecordCreateViewModel
            {
                Appointments =
                    await medicalRecordService
                        .GetAvailableAppointmentsAsync()
            };

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MedicalRecordCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadAppointmentsAsync(model);
                return View(model);
            }

            // -----------------------------------------------------
            // This method is already data-level authorized
            // inside MedicalRecordService.
            // -----------------------------------------------------

            var appointment =
                await medicalRecordService
                    .GetAppointmentByIdAsync(
                        model.AppointmentId);

            if (appointment == null)
            {
                ModelState.AddModelError(
                    nameof(model.AppointmentId),
                    "Selected appointment was not found.");

                await LoadAppointmentsAsync(model);

                return View(model);
            }

            var medicalRecord = new MedicalRecord
            {
                AppointmentId =
                    appointment.AppointmentId,

                PatientId =
                    appointment.PatientId,

                DoctorId =
                    appointment.DoctorId,

                Symptoms =
                    model.Symptoms,

                Diagnosis =
                    model.Diagnosis,

                Treatment =
                    model.Treatment,

                Notes =
                    model.Notes,

                FollowUpDate =
                    model.FollowUpDate
            };

            var result =
                await medicalRecordService
                    .CreateAsync(
                        medicalRecord);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create medical record. " +
                    "Please check the selected appointment " +
                    "or existing medical record.");

                await LoadAppointmentsAsync(model);

                return View(model);
            }

            TempData["Success"] =
                "Medical record created successfully.";

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var medicalRecord =
                await medicalRecordService
                    .GetByIdAsync(id);

            if (medicalRecord == null)
                return NotFound();

            var model =
                new MedicalRecordEditViewModel
                {
                    MedicalRecordId =
                        medicalRecord.MedicalRecordId,

                    Symptoms =
                        medicalRecord.Symptoms,

                    Diagnosis =
                        medicalRecord.Diagnosis,

                    Treatment =
                        medicalRecord.Treatment,

                    Notes =
                        medicalRecord.Notes,

                    FollowUpDate =
                        medicalRecord.FollowUpDate
                };

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            MedicalRecordEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var medicalRecord =
                await medicalRecordService
                    .GetByIdAsync(
                        model.MedicalRecordId);

            if (medicalRecord == null)
                return NotFound();

            // -----------------------------------------------------
            // Only medical information can be edited.
            //
            // AppointmentId
            // PatientId
            // DoctorId
            //
            // are preserved by the Service.
            // -----------------------------------------------------

            medicalRecord.Symptoms =
                model.Symptoms;

            medicalRecord.Diagnosis =
                model.Diagnosis;

            medicalRecord.Treatment =
                model.Treatment;

            medicalRecord.Notes =
                model.Notes;

            medicalRecord.FollowUpDate =
                model.FollowUpDate;

            var result =
                await medicalRecordService
                    .UpdateAsync(
                        medicalRecord);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update medical record.");

                return View(model);
            }

            TempData["Success"] =
                "Medical record updated successfully.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id =
                        medicalRecord.MedicalRecordId
                });
        }

        // =========================================================
        // LOAD APPOINTMENTS
        // =========================================================

        private async Task LoadAppointmentsAsync(
            MedicalRecordCreateViewModel model)
        {
            model.Appointments =
                await medicalRecordService
                    .GetAvailableAppointmentsAsync();
        }
    }
}