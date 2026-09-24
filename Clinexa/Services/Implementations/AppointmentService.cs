using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository appointmentRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository)
        {
            this.appointmentRepository = appointmentRepository;
        }

        public async Task<bool> CreateAsync(Appointment appointment)
        {
            if (appointment.AppointmentDate.Date < DateTime.Today)
                return false;

            if (appointment.StartTime >= appointment.EndTime)
                return false;

            bool doctorExists =
                await appointmentRepository.DoctorExistsAsync(
                    appointment.DoctorId);

            if (!doctorExists)
                return false;

            bool patientExists =
                await appointmentRepository.PatientExistsAsync(
                    appointment.PatientId);

            if (!patientExists)
                return false;

            bool isAvailable =
                await appointmentRepository.IsDoctorAvailableAsync(
                    appointment.DoctorId,
                    appointment.AppointmentDate.DayOfWeek,
                    appointment.StartTime,
                    appointment.EndTime);

            if (!isAvailable)
                return false;

            bool hasConflict =
                await appointmentRepository.HasConflictAsync(
                    appointment.DoctorId,
                    appointment.AppointmentDate,
                    appointment.StartTime,
                    appointment.EndTime);

            if (hasConflict)
                return false;

            appointment.AppointmentStatus =
                AppointmentStatus.Scheduled;

            appointment.CreatedAt =
                DateTime.UtcNow;

            await appointmentRepository.AddAsync(appointment);
            await appointmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(Appointment appointment)
        {
            var existingAppointment =
                await appointmentRepository.GetByIdAsync(
                    appointment.AppointmentId);

            if (existingAppointment == null)
                return false;

            // Appointment details can only be changed
            // before the patient checks in.
            if (existingAppointment.AppointmentStatus != AppointmentStatus.Scheduled &&
                existingAppointment.AppointmentStatus != AppointmentStatus.Confirmed)
            {
                return false;
            }

            if (appointment.AppointmentDate.Date < DateTime.Today)
                return false;

            if (appointment.StartTime >= appointment.EndTime)
                return false;

            bool doctorExists =
                await appointmentRepository.DoctorExistsAsync(
                    appointment.DoctorId);

            if (!doctorExists)
                return false;

            bool patientExists =
                await appointmentRepository.PatientExistsAsync(
                    appointment.PatientId);

            if (!patientExists)
                return false;

            bool isAvailable =
                await appointmentRepository.IsDoctorAvailableAsync(
                    appointment.DoctorId,
                    appointment.AppointmentDate.DayOfWeek,
                    appointment.StartTime,
                    appointment.EndTime);

            if (!isAvailable)
                return false;

            bool hasConflict =
                await appointmentRepository.HasConflictAsync(
                    appointment.DoctorId,
                    appointment.AppointmentDate,
                    appointment.StartTime,
                    appointment.EndTime,
                    appointment.AppointmentId);

            if (hasConflict)
                return false;

            // Preserve the existing workflow state.
            appointment.AppointmentStatus =
                existingAppointment.AppointmentStatus;

            appointment.ConfirmedAt =
                existingAppointment.ConfirmedAt;

            appointment.CheckedInAt =
                existingAppointment.CheckedInAt;

            appointment.ConsultationStartedAt =
                existingAppointment.ConsultationStartedAt;

            appointment.CompletedAt =
                existingAppointment.CompletedAt;

            appointment.CancelledAt =
                existingAppointment.CancelledAt;

            appointment.CancellationReason =
                existingAppointment.CancellationReason;

            appointment.CreatedAt =
                existingAppointment.CreatedAt;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.Scheduled)
                return false;

            appointment.AppointmentStatus =
                AppointmentStatus.Confirmed;

            appointment.ConfirmedAt =
                DateTime.UtcNow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CheckInAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.Confirmed)
                return false;

            appointment.AppointmentStatus =
                AppointmentStatus.CheckedIn;

            appointment.CheckedInAt =
                DateTime.UtcNow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> StartConsultationAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.CheckedIn)
                return false;

            appointment.AppointmentStatus =
                AppointmentStatus.InConsultation;

            appointment.ConsultationStartedAt =
                DateTime.UtcNow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CompleteAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.InConsultation)
                return false;

            appointment.AppointmentStatus =
                AppointmentStatus.Completed;

            appointment.CompletedAt =
                DateTime.UtcNow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CancelAsync(
            int id,
            string? cancellationReason)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.Scheduled &&
                appointment.AppointmentStatus != AppointmentStatus.Confirmed &&
                appointment.AppointmentStatus != AppointmentStatus.CheckedIn)
            {
                return false;
            }

            appointment.AppointmentStatus =
                AppointmentStatus.Cancelled;

            appointment.CancelledAt =
                DateTime.UtcNow;

            appointment.CancellationReason =
                cancellationReason?.Trim();

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MarkAsNoShowAsync(int id)
        {
            var appointment =
                await appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
                return false;

            if (appointment.AppointmentStatus != AppointmentStatus.Scheduled &&
                appointment.AppointmentStatus != AppointmentStatus.Confirmed)
            {
                return false;
            }

            appointment.AppointmentStatus =
                AppointmentStatus.NoShow;

            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<List<Appointment>> GetAllAsync()
        {
            return await appointmentRepository.GetAllAsync();
        }

        public async Task<List<Appointment>> GetByDoctorIdAsync(
            int doctorId)
        {
            return await appointmentRepository
                .GetByDoctorIdAsync(doctorId);
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await appointmentRepository
                .GetByIdAsync(id);
        }

        public async Task<List<Appointment>> GetByPatientIdAsync(
            int patientId)
        {
            return await appointmentRepository
                .GetByPatientIdAsync(patientId);
        }

        public async Task<(
            List<Appointment> Appointments,
            int TotalCount)> FilterAsync(
            string? search,
            int? doctorId,
            int? patientId,
            DateTime? appointmentDate,
            AppointmentStatus? status,
            int page,
            int pageSize)
        {
            return await appointmentRepository.FilterAsync(
                search,
                doctorId,
                patientId,
                appointmentDate,
                status,
                page,
                pageSize);
        }

        public async Task<List<TimeSpan>> GetAvailableSlotsAsync(
            int doctorId,
            DateTime appointmentDate)
        {
            var dayOfWeek =
                appointmentDate.DayOfWeek;

            var schedules =
                await appointmentRepository
                    .GetDoctorSchedulesAsync(
                        doctorId,
                        dayOfWeek);

            if (!schedules.Any())
            {
                return new List<TimeSpan>();
            }

            var appointments =
                await appointmentRepository
                    .GetDoctorAppointmentsAsync(
                        doctorId,
                        appointmentDate);

            const int slotDurationMinutes = 30;

            var slots = new List<TimeSpan>();

            foreach (var schedule in schedules)
            {
                var currentTime =
                    schedule.StartTime;

                while (currentTime.Add(
                           TimeSpan.FromMinutes(
                               slotDurationMinutes))
                       <= schedule.EndTime)
                {
                    var slotEndTime =
                        currentTime.Add(
                            TimeSpan.FromMinutes(
                                slotDurationMinutes));

                    bool isBooked =
                        appointments.Any(x =>
                            x.StartTime < slotEndTime &&
                            x.EndTime > currentTime);

                    if (!isBooked)
                    {
                        slots.Add(currentTime);
                    }

                    currentTime = slotEndTime;
                }
            }

            return slots
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}