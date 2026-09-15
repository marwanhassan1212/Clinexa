using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Medicine;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    public class MedicineController : Controller
    {
        private readonly IMedicineService medicineService;
        public MedicineController(IMedicineService medicineService)
        {
            this.medicineService = medicineService;
        }
        // GET: Medicine
        public async Task<IActionResult> Index()
        {
            var medicines =
                await medicineService.GetAllAsync();

            return View(medicines);
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
                    "Unable to create medicine. A medicine with the same name may already exist."
                );

                return View(model);
            }

            TempData["Success"] =
                "Medicine created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Medicine/Edit/5
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
                MedicineId =
                    medicine.MedicineId,

                Name =
                    medicine.Name,

                GenericName =
                    medicine.GenericName,

                Description =
                    medicine.Description
            };

            return View(model);
        }

      
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
                    .GetByIdAsync(
                        model.MedicineId);

            if (medicine == null)
            {
                return NotFound();
            }

            medicine.Name =
                model.Name;

            medicine.GenericName =
                model.GenericName;

            medicine.Description =
                model.Description;

            var result =
                await medicineService
                    .UpdateAsync(medicine);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update medicine. A medicine with the same name may already exist."
                );

                return View(model);
            }

            TempData["Success"] =
                "Medicine updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeActivate(int id)
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

    }
}
