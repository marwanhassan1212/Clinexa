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
        private readonly IPatientService patientService;
        private readonly IDoctorService doctorService;
        private readonly IInvoiceService invoiceService;

        public AppointmentController(IAppointmentService appointmentService
            , IPatientService patientService, IDoctorService doctorService, IInvoiceService invoiceService)
        {
            this.appointmentService = appointmentService;
            this.patientService = patientService;
            this.doctorService = doctorService;
            this.invoiceService = invoiceService;
        }
        public async Task<IActionResult> Index(AppointmentFilterViewModel model)
        {
            if (model.Page < 1)
            {
                model.Page = 1;
            }

            if (model.PageSize <= 0)
            {
                model.PageSize = 10;
            }

            var result = await appointmentService.FilterAsync(
                model.Search,
                model.DoctorId,
                model.PatientId,
                model.AppointmentDate,
                model.Status,
                model.Page,
                model.PageSize);

            model.Appointments = result.Appointments;
            model.TotalCount = result.TotalCount;

            model.Doctors = await doctorService.GetAllAsync();
            model.Patients = await patientService.GetAllAsync();

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var appointment =
                await appointmentService.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            var invoice =
                await invoiceService
                    .GetByAppointmentIdAsync(id);

            var model = new AppointmentDetailsViewModel
            {
                Appointment = appointment,
                Invoice = invoice
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateEditDataAsync();

            return View(new AppointmentCreateViewModel
            {
                AppointmentDate = DateTime.Today
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
     AppointmentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCreateEditDataAsync();

                return View(model);
            }


            // Appointment duration is currently fixed at 30 minutes.
            var endTime = model.StartTime.Add(
                TimeSpan.FromMinutes(30));


            var appointment = new Appointment
            {
                PatientId = model.PatientId,

                DoctorId = model.DoctorId,

                AppointmentDate = model.AppointmentDate.Date,

                StartTime = model.StartTime,

                EndTime = endTime,

                Reason = model.Reason,

                Notes = model.Notes
            };


            var result = await appointmentService.CreateAsync(
                appointment);


            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create appointment. The selected time may no longer be available."
                );

                await LoadCreateEditDataAsync();

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
                EndTime = appointmentExists.EndTime,
                Notes = appointmentExists.Notes,
                Reason = appointmentExists.Reason
            };
            await LoadCreateEditDataAsync();
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppointmentEditViewModel model)
        {
            if(!ModelState.IsValid)
            {
                await LoadCreateEditDataAsync();
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
            appointment.Reason = model.Reason;
            appointment.Notes = model.Notes;

            var result = await appointmentService.UpdateAsync(appointment);
            if(!result)
            {
                ModelState.AddModelError(
                   "",
                   "Unable to update appointment. Please check the doctor, patient, schedule, or appointment time."
               );
                await LoadCreateEditDataAsync();
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
        private async Task LoadCreateEditDataAsync()
        {
            ViewBag.Doctors = await doctorService.GetAllAsync();
            ViewBag.Patients = await patientService.GetAllAsync();
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(
            int doctorId,
            DateTime appointmentDate)
        {
            var slots = await appointmentService.GetAvailableSlotsAsync(
                doctorId,
                appointmentDate);

            var result = slots.Select(x => new
            {
                value = x.ToString(@"hh\:mm"),
                text = x.ToString(@"hh\:mm")
            });

            return Json(result);
        }

      
    }
}
