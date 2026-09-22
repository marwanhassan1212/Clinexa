using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.MedicalRecord;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordService medicalRecordService;

        public MedicalRecordController(
            IMedicalRecordService medicalRecordService)
        {
            this.medicalRecordService = medicalRecordService;
        }

        public async Task<IActionResult> Index()
        {
            var medicalRecords =
                await medicalRecordService.GetAllAsync();

            return View(medicalRecords);
        }

        public async Task<IActionResult> Details(int id)
        {
            var medicalRecord =
                await medicalRecordService.GetByIdAsync(id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            return View(medicalRecord);
        }

       
        // Create
        

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
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                AppointmentId = appointment.AppointmentId,

                Symptoms = model.Symptoms,
                Diagnosis = model.Diagnosis,
                Treatment = model.Treatment,
                Notes = model.Notes,
                FollowUpDate = model.FollowUpDate,

                CreatedAt = DateTime.UtcNow
            };

            var result =
                await medicalRecordService
                    .CreateAsync(medicalRecord);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create medical record. " +
                    "Please check the appointment or existing medical record.");

                await LoadAppointmentsAsync(model);

                return View(model);
            }

            TempData["Success"] =
                "Medical record created successfully.";

            return RedirectToAction(nameof(Index));
        }

        
        // Edit
        public async Task<IActionResult> Edit(int id)
        {
            var medicalRecord =
                await medicalRecordService
                    .GetByIdAsync(id);

            if (medicalRecord == null)
            {
                return NotFound();
            }

            var model = new MedicalRecordEditViewModel
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            MedicalRecordEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var medicalRecord =
                await medicalRecordService
                    .GetByIdAsync(
                        model.MedicalRecordId);

            if (medicalRecord == null)
            {
                return NotFound();
            }

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

            medicalRecord.UpdatedAt =
                DateTime.UtcNow;

            var result =
                await medicalRecordService
                    .UpdateAsync(medicalRecord);

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
                    id = medicalRecord.MedicalRecordId
                });
        }

   

        private async Task LoadAppointmentsAsync(
            MedicalRecordCreateViewModel model)
        {
            model.Appointments =
                await medicalRecordService
                    .GetAvailableAppointmentsAsync();
        }
    }
}