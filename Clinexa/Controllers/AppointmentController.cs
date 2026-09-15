using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Appointment;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Clinexa.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            this.appointmentService = appointmentService;
        }
        public async Task<IActionResult> Index()
        {
            var appointment = await appointmentService.GetAllAsync();
            return View(appointment);
        }

        public async Task<IActionResult> Details(int id)
        {
            var appointment = await appointmentService.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            return View(appointment);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var appointment = new Appointment()
            {
                PatientId = model.PatientId,
                DoctorId = model.DoctorId,
                AppointmentDate = model.AppointmentDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime

            };
            var result = await appointmentService.CreateAsync(appointment);
            if(!result)
            {
                ModelState.AddModelError(
              "",
              "Unable to create appointment. Please check the doctor, patient, schedule, or appointment time."
          );

                return View(model);
            }
            TempData["Success"] =
               "Appointment created successfully.";

            return RedirectToAction(nameof(Index));
        }

        //GET
        public async Task<IActionResult> Edit(int id)
        {
            var appointmentExists = await appointmentService.GetByIdAsync(id);
            if(appointmentExists == null)
            {
                return NotFound();
            }
            var appointment = new AppointmentEditViewModel()
            {
                AppointmentId = appointmentExists.AppointmentId,
                PatientId = appointmentExists.PatientId,
                DoctorId = appointmentExists.DoctorId,
                AppointmentDate = appointmentExists.AppointmentDate,
                StartTime = appointmentExists.StartTime,
                EndTime = appointmentExists.EndTime
            };
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppointmentEditViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            var appointment = await appointmentService.GetByIdAsync(model.AppointmentId);
            if (appointment == null)
            {
                return NotFound();
            }
            appointment.PatientId = model.PatientId;
            appointment.DoctorId = model.DoctorId;
            appointment.AppointmentDate = model.AppointmentDate;
            appointment.StartTime = model.StartTime;
            appointment.EndTime = model.EndTime;

            var result = await appointmentService.UpdateAsync(appointment);
            if(!result)
            {
                ModelState.AddModelError(
                   "",
                   "Unable to update appointment. Please check the doctor, patient, schedule, or appointment time."
               );
                return View(model);
            }
            TempData["Success"] =
                "Appointment updated successfully.";

            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var result =
                await appointmentService.CancelAsync(id);

            if (!result)
            {
                return NotFound();
            }

            TempData["Success"] =
                "Appointment cancelled successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
