using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Prescription;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly IPrescriptionService prescriptionService;
        public PrescriptionController(IPrescriptionService prescriptionService)
        {
            this.prescriptionService = prescriptionService;
        }
        public async Task<IActionResult> Index()
        {
            var prescriptions = await prescriptionService.GetAllAsync();
            return View(prescriptions);
        }

        public async Task<IActionResult> Details(int id)
        {
            var prescription = await prescriptionService.GetByIdAsync(id);
            if(prescription == null)
            {
                return NotFound();
            }
            return View(prescription);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PrescriptionCreateViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var prescription = new Prescription()
            {
                PrescriptionDate = model.PrescriptionDate,
                Notes = model.Notes,
                MedicalRecordId = model.MedicalRecordId
            };
            var result = await prescriptionService.CreateAsync(prescription);
            if(!result)
            {
                ModelState.AddModelError(
                "",
                "Unable to create prescription. Please check the medical record, prescription date, or existing prescription."
            );
                return View(model);
            }
            TempData["Success"] =
               "Prescription created successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var prescription = await prescriptionService.GetByIdAsync(id);
            if(prescription == null)
            {
                return NotFound();
            }
            var model = new PrescriptionEditViewModel()
            {
                PrescriptionId = prescription.PrescriptionId,
                PrescriptionDate = prescription.PrescriptionDate,
                Notes = prescription.Notes,
                MedicalRecordId = prescription.MedicalRecordId
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PrescriptionEditViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var prescription = await prescriptionService.GetByIdAsync(model.PrescriptionId);
            if(prescription == null)
            {
                return NotFound();
            }

            prescription.PrescriptionDate = model.PrescriptionDate;
            prescription.Notes = model.Notes;
            prescription.MedicalRecordId = model.MedicalRecordId;

            var result = await prescriptionService.UpdateAsync(prescription);
            if(!result)
            {
                ModelState.AddModelError(
            "",
            "Unable to update prescription. Please check the medical record, prescription date, or existing prescription."
        );

                return View(model);
            }


            TempData["Success"] =
                "Prescription updated successfully.";

            return RedirectToAction(nameof(Index));

        }

    }
}
