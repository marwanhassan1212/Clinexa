using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Doctor;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    public class DoctorController : Controller
    {
        private readonly IDoctorService doctorService;
        public DoctorController(IDoctorService doctorService)
        {
            this.doctorService = doctorService;
        }
        public async Task<IActionResult> Index()
        {
            var doctors = await doctorService.GetAllAsync();
            return View(doctors);
        }

        public async Task<IActionResult> Details(int id)
        {
            var doctor = await doctorService.GetByIdAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            else
            {
                return View(doctor);
            }
        }

        // GET
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateViewModel doctor)
        {
            if(ModelState.IsValid)
            {
                var createDoctor = new Doctor()
                {
                    UserId = doctor.UserId,
                    SpecialityId = doctor.SpecialityId,
                    ConsultationFee = doctor.ConsultationFee
                };
                var result = await doctorService.CreateAsync(createDoctor);
                if(!result)
                {
                    ModelState.AddModelError(
                   "",
                   "Unable to create doctor. The selected user may already be assigned to a doctor or the speciality may not exist."
                    );

                    return View(doctor);
                }
                else
                {
                    TempData["Success"] = "Doctor created successfully.";
                    return RedirectToAction("Index");
                }    
            }
            else
            {
                return View(doctor);
            }
        }

        // Get 
        public async Task<IActionResult> Edit(int id)
        {
            var doctor = await doctorService.GetByIdAsync(id);

            if (doctor == null)
            {
                return NotFound();
            }

            var model = new EditViewModel
            {
                DoctorId = doctor.DoctorId,
                SpecialityId = doctor.SpecialityId,
                ConsultationFee = doctor.ConsultationFee
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var doctor = await doctorService.GetByIdAsync(model.DoctorId);

            if (doctor == null)
            {
                return NotFound();
            }

            doctor.SpecialityId = model.SpecialityId;
            doctor.ConsultationFee = model.ConsultationFee;

            var result = await doctorService.UpdateAsync(doctor);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "Doctor updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var doctor = await doctorService.DeactivateAsync(id);
            if(!doctor)
            {
                return NotFound();
            }
            else
            {
                TempData["Success"] = "Doctor deactivated successfully.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
