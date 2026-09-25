using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Medicine;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "OperationalAccess")]
    public class MedicineController : Controller
    {
        private readonly IMedicineService medicineService;

        public MedicineController(IMedicineService medicineService)
        {
            this.medicineService = medicineService;
        }

        // GET: Medicine
        public async Task<IActionResult> Index(
            MedicineFilterViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            if (model.PageSize <= 0)
            {
                model.PageSize = 10;
            }

            var result =
                await medicineService.FilterAsync(
                    model.Search,
                    model.IsActive,
                    model.Page,
                    model.PageSize);

            model.Medicines = result.Medicines;
            model.TotalCount = result.TotalCount;

            return View(model);
        }

        // GET: Medicine/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var medicine =
                await medicineService.GetByIdAsync(id);

            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        // GET: Medicine/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Medicine/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MedicineCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var medicine = new Medicine
            {
                Name = model.Name,
                GenericName = model.GenericName,
                Description = model.Description
            };

            var result =
                await medicineService
                    .CreateAsync(medicine);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create medicine. A medicine with the same name may already exist.");

                return View(model);
            }

            TempData["Success"] =
                "Medicine created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Medicine/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var medicine =
                await medicineService
                    .GetByIdAsync(id);

            if (medicine == null)
            {
                return NotFound();
            }

            var model = new MedicineEditViewModel
            {
                MedicineId = medicine.MedicineId,
                Name = medicine.Name,
                GenericName = medicine.GenericName,
                Description = medicine.Description
            };

            return View(model);
        }

        // POST: Medicine/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            MedicineEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var medicine =
                await medicineService
                    .GetByIdAsync(model.MedicineId);

            if (medicine == null)
            {
                return NotFound();
            }

            // Edit only medicine information.
            // IsActive is controlled by Activate/Deactivate.
            medicine.Name = model.Name;
            medicine.GenericName = model.GenericName;
            medicine.Description = model.Description;

            var result =
                await medicineService
                    .UpdateAsync(medicine);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update medicine. A medicine with the same name may already exist.");

                return View(model);
            }

            TempData["Success"] =
                "Medicine updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Medicine/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result =
                await medicineService
                    .DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Medicine deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Medicine/Activate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var result =
                await medicineService
                    .ActivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Medicine activated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}