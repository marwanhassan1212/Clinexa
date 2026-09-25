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

        public async Task<IActionResult> Index()
        {
            var currentUserId = int.Parse(
                userManager.GetUserId(User)!);

            var user = await userManager.FindByIdAsync(
                currentUserId.ToString());

            if (user == null)
                return Forbid();

            List<MedicalRecord> medicalRecords;

            if (await userManager.IsInRoleAsync(user, "Admin"))
            {
                medicalRecords =
                    await medicalRecordService.GetAllAsync();
            }
            else
            {
                var doctorId = await medicalRecordService.GetDoctorIdByUserIdAsync(currentUserId);
                if (!doctorId.HasValue)
                    return Forbid();

                medicalRecords =
                    await medicalRecordService
                        .GetByDoctorIdAsync(doctorId.Value);
            }

            return View(medicalRecords);
        }


        // Details

        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = int.Parse(
                userManager.GetUserId(User)!);

            var canAccess =
                await medicalRecordService.CanAccessAsync(
                    id,
                    currentUserId);

            if (!canAccess)
                return Forbid();

            var medicalRecord =
                await medicalRecordService.GetByIdAsync(id);

            if (medicalRecord == null)
                return NotFound();

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

            var currentUserId = int.Parse(
                userManager.GetUserId(User)!);

            var currentUser =
                await userManager.FindByIdAsync(
                    currentUserId.ToString());

            if (currentUser == null)
                return Forbid();

            // Doctor can create a medical record
            // only for his own appointment.
            if (await userManager.IsInRoleAsync(
                    currentUser,
                    "Doctor"))
            {
                if (currentUser.Doctor == null ||
                    currentUser.Doctor.DoctorId != appointment.DoctorId)
                {
                    return Forbid();
                }
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
            var currentUserId = int.Parse(
                userManager.GetUserId(User)!);

            var canAccess =
                await medicalRecordService.CanAccessAsync(
                    id,
                    currentUserId);

            if (!canAccess)
                return Forbid();

            var medicalRecord =
                await medicalRecordService
                    .GetByIdAsync(id);

            if (medicalRecord == null)
                return NotFound();

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
                return View(model);

            var currentUserId = int.Parse(
                userManager.GetUserId(User)!);

            var canAccess =
                await medicalRecordService.CanAccessAsync(
                    model.MedicalRecordId,
                    currentUserId);

            if (!canAccess)
                return Forbid();

            var medicalRecord =
                await medicalRecordService
                    .GetByIdAsync(
                        model.MedicalRecordId);

            if (medicalRecord == null)
                return NotFound();

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