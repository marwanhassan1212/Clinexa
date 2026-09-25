using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Role;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class RoleController : Controller
    {
        private readonly IRoleService roleService;

        public RoleController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await roleService.GetAllAsync();

            return View(roles);
        }

        public async Task<IActionResult> Details(int id)
        {
            var role = await roleService.GetByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = new Role
            {
                Name = model.Name,
                Description = model.Description
            };

            var result = await roleService.CreateAsync(role);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A role with this name already exists."
                );

                return View(model);
            }

            TempData["Success"] = "Role created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var role = await roleService.GetByIdAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            var model = new RoleEditViewModel
            {
                RoleId = role.Id,
                Name = role.Name,
                Description = role.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = await roleService.GetByIdAsync(model.RoleId);

            if (role == null)
            {
                return NotFound();
            }

            role.Name = model.Name;
            role.Description = model.Description;

            var result = await roleService.UpdateAsync(role);

            if (!result)
            {
                ModelState.AddModelError(
                    nameof(model.Name),
                    "A role with this name already exists."
                );

                return View(model);
            }

            TempData["Success"] = "Role updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
