using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Doctor;
using Clinexa.Models.ViewModels.Speciality;
using Clinexa.Services.Implementations;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    public class SpecialityController : Controller
    {
        private readonly SpecialityService specialityService;
        public SpecialityController(SpecialityService specialityService)
        {
            this.specialityService = specialityService;
        }
        public async Task<IActionResult> Index()
        {
            var specialities = await specialityService.GetAllAsync();
            return View(specialities);
        }

        // GET: /Speciality/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var speciality = await specialityService.GetByIdAsync(id);

            if (speciality == null)
            {
                return NotFound();
            }

            return View(speciality);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpecialityCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var speciality = new Speciality
            {
                Name = model.Name,
                Description = model.Description
            };

            var result = await specialityService.CreateAsync(speciality);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A speciality with this name already exists."
                );

                return View(model);
            }

            TempData["Success"] = "Speciality created successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var speciality = await specialityService.GetByIdAsync(id);

            if (speciality == null)
            {
                return NotFound();
            }

            var model = new SpecialityEditViewModel
            {
                SpecialityId = speciality.SpecialityId,
                Name = speciality.Name,
                Description = speciality.Description
            };

            return View(model);
        }

        // POST: /Speciality/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SpecialityEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var speciality = await specialityService
                .GetByIdAsync(model.SpecialityId);

            if (speciality == null)
            {
                return NotFound();
            }

            speciality.Name = model.Name;
            speciality.Description = model.Description;

            var result = await specialityService.UpdateAsync(speciality);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A speciality with this name already exists."
                );

                return View(model);
            }

            TempData["Success"] = "Speciality updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await specialityService.DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "Speciality deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
