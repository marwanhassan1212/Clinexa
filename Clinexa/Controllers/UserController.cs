using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.User;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public UserController(
            IUserService userService,
            IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        // =========================
        // Index
        // =========================

        public async Task<IActionResult> Index(
            UserFilterViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            var result = await _userService.FilterAsync(
                model.Search,
                model.RoleId,
                model.IsActive,
                model.Page,
                model.PageSize);

            model.Users = result.Users;
            model.TotalCount = result.TotalCount;

            model.Roles = await _roleService.GetAllAsync();

            model.RoleNames = await _userService.GetRoleNamesAsync(
                model.Users.Select(x => x.Id));

            return View(model);
        }


        // =========================
        // Details
        // =========================

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.RoleName =
                await _userService.GetRoleNameAsync(id);

            return View(user);
        }


        // =========================
        // Create - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new UserCreateViewModel
            {
                Roles = await _roleService.GetAllAsync()
            };

            return View(model);
        }


        // =========================
        // Create - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await _roleService.GetAllAsync();
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userService.CreateAsync(
                user,
                model.Password,
                model.RoleId);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create user.");

                model.Roles = await _roleService.GetAllAsync();

                return View(model);
            }

            TempData["Success"] = "User created successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // Edit - GET
        // =========================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleService.GetAllAsync();

            var currentRoleId =
                await _userService.GetRoleIdAsync(id);

            var model = new UserEditViewModel
            {
                UserId = user.Id,

                FirstName = user.FirstName,

                LastName = user.LastName,

                Email = user.Email!,

                PhoneNumber = user.PhoneNumber!,

                RoleId = currentRoleId ?? 0,

                Roles = roles
            };

            return View(model);
        }


        // =========================
        // Edit - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await _roleService.GetAllAsync();

                return View(model);
            }

            var user = await _userService.GetByIdAsync(
                model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;

            user.LastName = model.LastName;

            user.Email = model.Email;

            user.PhoneNumber = model.PhoneNumber;

            var result = await _userService.UpdateAsync(
                user,
                model.RoleId);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update user. " +
                    "The email or phone number may already exist.");

                model.Roles = await _roleService.GetAllAsync();

                return View(model);
            }

            TempData["Success"] =
                "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // Deactivate
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result =
                await _userService.DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "User deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }


        // =========================
        // Activate
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var result =
                await _userService.Activate(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "User activated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> SetPassword(int id)
        {
            var user = await _userService.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            return View(new UserSetPasswordViewModel
            {
                UserId = id
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(
            UserSetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _userService.SetPasswordAsync(
                model.UserId,
                model.Password);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to set password for this user.");

                return View(model);
            }

            TempData["Success"] =
                "Password has been set successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.UserId });
        }
    }
}