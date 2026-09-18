using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.DoctorSchedule;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    public class DoctorScheduleController : Controller
    {
        private readonly IDoctorScheduleService doctorScheduleService;
        private readonly IDoctorService doctorService;
        public DoctorScheduleController(IDoctorScheduleService doctorScheduleService
            ,IDoctorService doctorService)
        {
            this.doctorScheduleService = doctorScheduleService;
            this.doctorService = doctorService;
        }
        public async Task<IActionResult> Index(DoctorScheduleFilterViewModel model)
        {
            var result = await doctorScheduleService.FilterAsync(
                model.Search,
                model.DoctorId,
                model.DayOfWeek,
                model.IsAvailable,
                model.Page,
                model.PageSize);

            model.Schedules = result.Schedules;
            model.TotalCount = result.TotalCount;

            model.Doctors = await doctorService.GetAllAsync();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var schedule = await doctorScheduleService.GetByIdAsync(id);

            if (schedule == null)
            {
                return NotFound();
            }
            await doctorService.GetByIdAsync(id);
            return View(schedule);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Doctors = await doctorService.GetAllAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorScheduleCreateViewModel model)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.Doctors = await doctorService.GetAllAsync();
                return View(model);
            }
            var doctorSchedule = new DoctorSchedule()
            {
                DoctorId = model.DoctorId,
                DayOfWeek = model.DayOfWeek,
                StartTime = model.StartTime,
                EndTime = model.EndTime
            };
            var result = await doctorScheduleService.CreateAsync(doctorSchedule);
            if(!result)
            {
                ModelState.AddModelError(
               "",
               "Unable to create schedule. Please check the doctor, time, or duplicate schedule."
           );
                ViewBag.Doctors = await doctorService.GetAllAsync();
                return View(model);
            }
            else
            {
                TempData["Success"] = "Doctor schedule created successfully.";

                return RedirectToAction(nameof(Index));
            }
        }
        
        // GET
        public async Task<IActionResult> Edit(int id)
        {
            var Schedule = await doctorScheduleService.GetByIdAsync(id);
            if(Schedule == null)
            {
                return NotFound();
            }
            var doctorSchedule = new DoctorScheduleEditViewModel()
            {
                DoctorScheduleId = Schedule.DoctorScheduleId,
                DoctorId = Schedule.DoctorId,
                DayOfWeek = Schedule.DayOfWeek,
                StartTime = Schedule.StartTime,
                EndTime = Schedule.EndTime
            };
            ViewBag.Doctors = await doctorService.GetAllAsync();
            return View(doctorSchedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorScheduleEditViewModel model)
        {
            if(!ModelState.IsValid)
            {
                ViewBag.Doctors = await doctorService.GetAllAsync();
                return View(model);
            }
            var schedule = await doctorScheduleService.GetByIdAsync(model.DoctorScheduleId);
            if(schedule == null)
            {
                return NotFound();
            }

            schedule.DoctorId = model.DoctorId;
            schedule.DayOfWeek = model.DayOfWeek;
            schedule.StartTime = model.StartTime;
            schedule.EndTime = model.EndTime;

            var result = await doctorScheduleService
                .UpdateAsync(schedule);

            if(!result)
            {
                ModelState.AddModelError(
              "",
              "Unable to update schedule. Please check the doctor, time, or duplicate schedule."
                );
                ViewBag.Doctors = await doctorService.GetAllAsync();
                return View(model);
            }
            else
            {
                TempData["Success"] = "Doctor schedule updated successfully.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await doctorScheduleService
                .DeactivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "Doctor schedule deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await doctorScheduleService.ActivateAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] = "Doctor schedule activated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
