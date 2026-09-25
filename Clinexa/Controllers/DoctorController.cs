using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Doctor;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class DoctorController : Controller
    {
        private readonly IDoctorService doctorService;
        private readonly ISpecialityService specialityService;
        private readonly IUserService userService;
        public DoctorController(IDoctorService doctorService , ISpecialityService specialityService
            , IUserService userService)
        {
            this.doctorService = doctorService;
            this.specialityService = specialityService;
            this.userService = userService;
        }
        public async Task<IActionResult> Index(DoctorFilterViewModel model)
        {
            var result = await doctorService.FilterAsync(
               model.Search,
               model.SpecialityId,
               model.IsActive,
               model.Page,
               model.PageSize);

               model.Doctors = result.Doctors;
               model.TotalCount = result.TotalCount;
            model.Specialities = await specialityService.GetAllAsync();

            return View(model);
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
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var users = await doctorService.GetAvailableUsersAsync();
            var specialities = await specialityService.GetAllAsync();

            var model = new DoctorCreateViewModel
            {
                Users = users,
                Specialities = specialities
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorCreateViewModel doctor)
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
                    doctor.Users = await userService.GetAllAsync();
                    doctor.Specialities = await specialityService.GetAllAsync();

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
                doctor.Users = await userService.GetAllAsync();
                doctor.Specialities = await specialityService.GetAllAsync();
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
                ConsultationFee = doctor.ConsultationFee,
                Specialities = await specialityService.GetAllAsync()
                
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Specialities = await specialityService.GetAllAsync();
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
                ModelState.AddModelError("", "Unable to update doctor.");
                model.Specialities = await specialityService.GetAllAsync();
                return View(model);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var activate = await doctorService.ActivateAsync(id);
            if(!activate)
            {
                return NotFound();
            }

            TempData["Success"] = "Doctor activated successfully.";

            return RedirectToAction("Index");

        }

        }
    }

