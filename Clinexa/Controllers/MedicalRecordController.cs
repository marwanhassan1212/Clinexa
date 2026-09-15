using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.MedicalRecord;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly IMedicalRecordService medicalRecordService;
        public MedicalRecordController(IMedicalRecordService medicalRecordService)
        {
            this.medicalRecordService = medicalRecordService;
        }
        public async Task<IActionResult> Index()
        {
            var medicalRecord = await medicalRecordService.GetAllAsync();
            return View(medicalRecord);
        }

        public async Task<IActionResult> Details(int id)
        {
            var medicalRecord = await medicalRecordService.GetByIdAsync(id);
            if(medicalRecord == null)
            {
                return NotFound();
            }
            return View(medicalRecord);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicalRecordCreateViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var medicalRecord = new MedicalRecord()
            {
                DoctorId = model.DoctorId,
                PatientId = model.PatientId,
                AppointmentId = model.AppointmentId
            };
            var result = await medicalRecordService.CreateAsync(medicalRecord);
            if(!result)
            {
                ModelState.AddModelError(
              "",
              "Unable to create medical record. Please check the patient, doctor, appointment, or existing medical record."
                );
                return View(model);
            }
            TempData["Success"] = "Medical record created successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> Edit(int id)
        {
            var medicalRecord = await medicalRecordService.GetByIdAsync(id);
            if(medicalRecord == null)
            {
                return NotFound();
            }
            var model = new MedicalRecordEditViewModel()
            {
                MedicalRecordId = medicalRecord.MedicalRecordId,
                PatientId = medicalRecord.PatientId,
                DoctorId = medicalRecord.DoctorId,
                AppointmentId = medicalRecord.AppointmentId

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MedicalRecordEditViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var medicalRecord = await medicalRecordService.GetByIdAsync(model.MedicalRecordId);
            if(medicalRecord == null)
            {
                return NotFound();
            }
            medicalRecord.PatientId = model.PatientId;
            medicalRecord.DoctorId = model.DoctorId;
            medicalRecord.AppointmentId = model.AppointmentId;
            var result = await medicalRecordService.UpdateAsync(medicalRecord);
            if(!result)
            {
                ModelState.AddModelError(
                "",
                "Unable to update medical record. Please check the patient, doctor, appointment, or existing medical record."
            );

                return View(model);
            }

            TempData["Success"] =
               "Medical record updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
