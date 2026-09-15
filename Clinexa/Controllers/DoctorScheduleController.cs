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
        public DoctorScheduleController(IDoctorScheduleService doctorScheduleService)
        {
            this.doctorScheduleService = doctorScheduleService;
        }
        public async Task<IActionResult> Index()
        {
            var schedules = await doctorScheduleService.GetAllAsync();
            return View(schedules);
        }

        public async Task<IActionResult> Details(int id)
        {
            var schedule = await doctorScheduleService.GetByIdAsync(id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorScheduleCreateViewModel model)
        {
            if(!ModelState.IsValid)
            {
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
            return View(doctorSchedule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DoctorScheduleEditViewModel model)
        {
            if(!ModelState.IsValid)
            {
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
    }
}
