using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.User;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService userService;
        public UserController(IUserService userService)
        {
            this.userService = userService;
        }
        public async Task<IActionResult> Index()
        {
            var user = await userService.GetAllAsync();
            return View(user);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            else
            {
                return View(user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                PasswordHash = model.Password,
                RoleId = model.RoleId
            };

            var result = await userService.CreateAsync(user);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create user. The email or phone number may already exist."
                );

                return View(model);
            }

            TempData["Success"] = "User created successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var model = new UserEditViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userService.GetByIdAsync(model.UserId);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.RoleId = model.RoleId;

            var result = await userService.UpdateAsync(user);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update user. The email or phone number may already exist."
                );

                return View(model);
            }

            TempData["Success"] = "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await userService.DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "User deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
