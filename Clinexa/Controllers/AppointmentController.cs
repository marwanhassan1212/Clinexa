using Clinexa.Models.Entities;
using Clinexa.Models.ViewModels.Appointment;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinexa.Controllers
{
    [Authorize(Policy = "OperationalAccess")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService appointmentService;
        private readonly IPatientService patientService;
        private readonly IDoctorService doctorService;
        private readonly IInvoiceService invoiceService;

        public AppointmentController(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDoctorService doctorService,
            IInvoiceService invoiceService)
        {
            this.appointmentService = appointmentService;
            this.patientService = patientService;
            this.doctorService = doctorService;
            this.invoiceService = invoiceService;
        }

        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index(
            AppointmentFilterViewModel model)
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
                await appointmentService.FilterAsync(
                    model.Search,
                    model.DoctorId,
                    model.PatientId,
                    model.AppointmentDate,
                    model.Status,
                    model.Page,
                    model.PageSize);

            model.Appointments =
                result.Appointments;

            model.TotalCount =
                result.TotalCount;

            // -----------------------------------------------------
            // Doctor
            // -----------------------------------------------------
            // Doctor must not receive all doctors as a filter.
            // The Service already forces the query to the
            // currently logged-in Doctor.
            // -----------------------------------------------------

            if (User.IsInRole("Doctor"))
            {
                var currentDoctorId =
                    await appointmentService
                        .GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                {
                    return Forbid();
                }

                var currentDoctor =
                    await doctorService
                        .GetByIdAsync(currentDoctorId.Value);

                model.Doctors =
                    currentDoctor == null
                        ? new List<Clinexa.Models.Entities.Doctor>()
                        : new List<Clinexa.Models.Entities.Doctor>
                        {
                            currentDoctor
                        };
            }
            else
            {
                model.Doctors =
                    await doctorService.GetAllAsync();
            }

            // -----------------------------------------------------
            // Patients
            // -----------------------------------------------------

            model.Patients =
                await patientService.GetAllAsync();

            return View(model);
        }

        // =========================================================
        // DETAILS
        // =========================================================

        public async Task<IActionResult> Details(int id)
        {
            /*
             * GetByIdAsync() performs Data-Level Authorization.
             *
             * If the current user is a Doctor and the appointment
             * belongs to another Doctor, the Service returns null.
             */

            var appointment =
                await appointmentService.GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            var invoice =
                await invoiceService
                    .GetByAppointmentIdAsync(id);

            var model =
                new AppointmentDetailsViewModel
                {
                    Appointment = appointment,
                    Invoice = invoice
                };

            return View(model);
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model =
                new AppointmentCreateViewModel
                {
                    AppointmentDate =
                        DateTime.Today
                };

            // -----------------------------------------------------
            // Doctor
            // -----------------------------------------------------

            if (User.IsInRole("Doctor"))
            {
                var currentDoctorId =
                    await appointmentService
                        .GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                {
                    return Forbid();
                }

                /*
                 * Force DoctorId to the logged-in Doctor.
                 *
                 * The value coming from the client will never
                 * be trusted by the Service.
                 */

                model.DoctorId =
                    currentDoctorId.Value;

                var currentDoctor =
                    await doctorService
                        .GetByIdAsync(
                            currentDoctorId.Value);

                ViewBag.Doctors =
                    currentDoctor == null
                        ? new List<Clinexa.Models.Entities.Doctor>()
                        : new List<Clinexa.Models.Entities.Doctor>
                        {
                            currentDoctor
                        };
            }
            else
            {
                ViewBag.Doctors =
                    await doctorService.GetAllAsync();
            }

            // -----------------------------------------------------
            // Patients
            // -----------------------------------------------------

            ViewBag.Patients =
                await patientService.GetAllAsync();

            return View(model);
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            AppointmentCreateViewModel model)
        {
            // -----------------------------------------------------
            // Doctor Data-Level Authorization
            // -----------------------------------------------------

            if (User.IsInRole("Doctor"))
            {
                var currentDoctorId =
                    await appointmentService
                        .GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                {
                    return Forbid();
                }

                /*
                 * Never trust DoctorId coming from the form.
                 *
                 * Force it to the logged-in Doctor.
                 */

                model.DoctorId =
                    currentDoctorId.Value;
            }

            // -----------------------------------------------------
            // Validation
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadCreateEditDataAsync();
                return View(model);
            }

            // -----------------------------------------------------
            // Appointment duration is fixed at 30 minutes.
            // -----------------------------------------------------

            var endTime =
                model.StartTime.Add(
                    TimeSpan.FromMinutes(30));

            var appointment =
                new Appointment
                {
                    PatientId =
                        model.PatientId,

                    DoctorId =
                        model.DoctorId,

                    AppointmentDate =
                        model.AppointmentDate.Date,

                    StartTime =
                        model.StartTime,

                    EndTime =
                        endTime,

                    Reason =
                        model.Reason,

                    Notes =
                        model.Notes
                };

            /*
             * CreateAsync() performs the final server-side
             * Data-Level Authorization.
             */

            var result =
                await appointmentService
                    .CreateAsync(
                        appointment);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to create appointment. " +
                    "The selected time may no longer be available.");

                await LoadCreateEditDataAsync();

                return View(model);
            }

            TempData["Success"] =
                "Appointment created successfully.";

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            /*
             * GetByIdAsync() performs Data-Level Authorization.
             *
             * Doctor cannot retrieve an appointment belonging
             * to another Doctor.
             */

            var appointment =
                await appointmentService
                    .GetByIdAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            var model =
                new AppointmentEditViewModel
                {
                    AppointmentId =
                        appointment.AppointmentId,

                    PatientId =
                        appointment.PatientId,

                    DoctorId =
                        appointment.DoctorId,

                    AppointmentDate =
                        appointment.AppointmentDate,

                    StartTime =
                        appointment.StartTime,

                    EndTime =
                        appointment.EndTime,

                    Notes =
                        appointment.Notes,

                    Reason =
                        appointment.Reason
                };

            await LoadCreateEditDataAsync();

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            AppointmentEditViewModel model)
        {
            // -----------------------------------------------------
            // Doctor Data-Level Authorization
            // -----------------------------------------------------

            if (User.IsInRole("Doctor"))
            {
                var currentDoctorId =
                    await appointmentService
                        .GetCurrentDoctorIdAsync();

                if (!currentDoctorId.HasValue)
                {
                    return Forbid();
                }

                /*
                 * Doctor cannot change the appointment owner.
                 *
                 * Even if the client sends another DoctorId,
                 * we replace it with the logged-in Doctor.
                 */

                model.DoctorId =
                    currentDoctorId.Value;
            }

            // -----------------------------------------------------
            // Validation
            // -----------------------------------------------------

            if (!ModelState.IsValid)
            {
                await LoadCreateEditDataAsync();
                return View(model);
            }

            /*
             * GetByIdAsync() performs Data-Level Authorization.
             */

            var appointment =
                await appointmentService
                    .GetByIdAsync(
                        model.AppointmentId);

            if (appointment == null)
            {
                return NotFound();
            }

            // -----------------------------------------------------
            // Update editable fields
            // -----------------------------------------------------

            appointment.PatientId =
                model.PatientId;

            /*
             * For Doctor, model.DoctorId was already forced
             * to the current Doctor.
             *
             * For Admin/Receptionist, the selected Doctor
             * is accepted and validated again inside Service.
             */

            appointment.DoctorId =
                model.DoctorId;

            appointment.AppointmentDate =
                model.AppointmentDate;

            appointment.StartTime =
                model.StartTime;

            appointment.EndTime =
                model.EndTime;

            appointment.Reason =
                model.Reason;

            appointment.Notes =
                model.Notes;

            /*
             * UpdateAsync() performs another ownership check.
             *
             * This protects the business operation even if
             * somebody bypasses the normal UI flow.
             */

            var result =
                await appointmentService
                    .UpdateAsync(
                        appointment);

            if (!result)
            {
                ModelState.AddModelError(
                    "",
                    "Unable to update appointment. " +
                    "Please check the doctor, patient, schedule, " +
                    "appointment time, or current appointment status.");

                await LoadCreateEditDataAsync();

                return View(model);
            }

            TempData["Success"] =
                "Appointment updated successfully.";

            return RedirectToAction(
                nameof(Index));
        }

        // =========================================================
        // CONFIRM
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var result =
                await appointmentService
                    .ConfirmAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "This appointment cannot be confirmed.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            TempData["Success"] =
                "Appointment confirmed successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // CHECK IN
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int id)
        {
            var result =
                await appointmentService
                    .CheckInAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "This appointment cannot be checked in.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            TempData["Success"] =
                "Patient checked in successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // START CONSULTATION
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartConsultation(
            int id)
        {
            var result =
                await appointmentService
                    .StartConsultationAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "The consultation cannot be started for this appointment.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            TempData["Success"] =
                "Consultation started successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // COMPLETE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var result =
                await appointmentService
                    .CompleteAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "This appointment cannot be completed.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            TempData["Success"] =
                "Appointment completed successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // NO SHOW
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NoShow(int id)
        {
            var result =
                await appointmentService
                    .MarkAsNoShowAsync(id);

            if (!result)
            {
                TempData["Error"] =
                    "This appointment cannot be marked as no-show.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            TempData["Success"] =
                "Appointment marked as no-show.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // CANCEL
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(
            int id,
            string? cancellationReason)
        {
            var result =
                await appointmentService
                    .CancelAsync(
                        id,
                        cancellationReason);

            if (!result)
            {
                TempData["Error"] =
                    "This appointment cannot be cancelled.";

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }

            TempData["Success"] =
                "Appointment cancelled successfully.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // =========================================================
        // AVAILABLE SLOTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(
            int doctorId,
            DateTime appointmentDate)
        {
            /*
             * AppointmentService validates that a Doctor
             * can only request slots for himself.
             */

            var slots =
                await appointmentService
                    .GetAvailableSlotsAsync(
                        doctorId,
                        appointmentDate);

            var result =
                slots.Select(x => new
                {
                    value =
                        x.ToString(@"hh\:mm"),

                    text =
                        x.ToString(@"hh\:mm")
                });

            return Json(result);
        }

        // =========================================================
        // VIEW DATA
        // =========================================================

        private async Task LoadCreateEditDataAsync()
        {
            // -----------------------------------------------------
            // Doctors
            // -----------------------------------------------------

            if (User.IsInRole("Doctor"))
            {
                var currentDoctorId =
                    await appointmentService
                        .GetCurrentDoctorIdAsync();

                if (currentDoctorId.HasValue)
                {
                    var currentDoctor =
                        await doctorService
                            .GetByIdAsync(
                                currentDoctorId.Value);

                    ViewBag.Doctors =
                        currentDoctor == null
                            ? new List<Clinexa.Models.Entities.Doctor>()
                            : new List<Clinexa.Models.Entities.Doctor>
                            {
                                currentDoctor
                            };
                }
                else
                {
                    ViewBag.Doctors =
                        new List<Clinexa.Models.Entities.Doctor>();
                }
            }
            else
            {
                ViewBag.Doctors =
                    await doctorService.GetAllAsync();
            }

            // -----------------------------------------------------
            // Patients
            // -----------------------------------------------------

            ViewBag.Patients =
                await patientService.GetAllAsync();
        }
    }
}