using Clinexa.Enums;
using Clinexa.Models.Entities;

using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository appointmentRepository;
        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            this.appointmentRepository = appointmentRepository;
        }
        public async Task<bool> CancelAsync(int id)
        {
            var appointment = await appointmentRepository.GetByIdAsync(id);
            if(appointment == null)
            {
                return false;
            }
            appointment.AppointmentStatus = Enums.AppointmentStatus.Cancelled;
            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateAsync(Appointment appointment)
        {
            if (appointment.AppointmentDate.Date < DateTime.Today)
                return false;

            if (appointment.StartTime >= appointment.EndTime)
            {
                return false;
            }

            var doctorExists = await appointmentRepository.DoctorExistsAsync(appointment.DoctorId);
            if(!doctorExists)
            {
                return false;
            }

            var patientExists = await appointmentRepository.PatientExistsAsync(appointment.PatientId);
            if(!patientExists)
            {
                return false;
            }

            bool isAvailable = await appointmentRepository.IsDoctorAvailableAsync(appointment.DoctorId
                , appointment.AppointmentDate.DayOfWeek,
                appointment.StartTime,
                appointment.EndTime);
            if(!isAvailable)
            {
                return false;
            }
            bool hasConflict =
             await appointmentRepository.HasConflictAsync(
                 appointment.DoctorId,
                 appointment.AppointmentDate,
                 appointment.StartTime,
                 appointment.EndTime);
            if(hasConflict)
            {
                return false;
            }
            appointment.AppointmentStatus = Enums.AppointmentStatus.Scheduled;
            appointment.CreatedAt = DateTime.UtcNow;
            await appointmentRepository.AddAsync(appointment);
            await appointmentRepository.SaveChangesAsync();
            return true;

        }

        public async Task<List<Appointment>> GetAllAsync()
        {
            return await appointmentRepository.GetAllAsync();
        }

        public async Task<List<Appointment>> GetByDoctorIdAsync(int doctorId)
        {
            return await appointmentRepository.GetByDoctorIdAsync(doctorId);
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await appointmentRepository.GetByIdAsync(id);
        }

        public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
        {
            return await appointmentRepository.GetByPatientIdAsync(patientId);
        }

        public async Task<bool> UpdateAsync(Appointment appointment)
        {
            var appointmentExists = await appointmentRepository.GetByIdAsync(appointment.AppointmentId);
            if(appointmentExists == null)
            {
                return false;
            }
            if(appointment.StartTime >= appointment.EndTime)
            {
                return false;
            }
            bool doctorExists = await appointmentRepository.DoctorExistsAsync(appointment.DoctorId);
            if (!doctorExists)
            {
                return false;
            }
            bool patientExists = await appointmentRepository.PatientExistsAsync(appointment.PatientId);
            if(!patientExists)
            {
                return false;
            }

            bool isAvaialble = await appointmentRepository.IsDoctorAvailableAsync(appointment.DoctorId
                , appointment.AppointmentDate.DayOfWeek,
                appointment.StartTime,
                appointment.EndTime
                );
            if(!isAvaialble)
            {
                return false;
            }

            bool hasConflict = await appointmentRepository.HasConflictAsync(
             appointment.DoctorId,
             appointment.AppointmentDate,
             appointment.StartTime,
             appointment.EndTime,
             appointment.AppointmentId);
            if (hasConflict)
            {
                return false;
            }
            appointmentRepository.Update(appointment);
            await appointmentRepository.SaveChangesAsync();
            return true;
        }

        public async Task<(List<Appointment> Appointments, int TotalCount)> FilterAsync(
             string? search,
             int? doctorId,
             int? patientId,
             DateTime? appointmentDate,
             Enums.AppointmentStatus? status,
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

        public async Task<List<TimeSpan>> GetAvailableSlotsAsync(int doctorId, DateTime appointmentDate)
        {
            var dayOfWeek = appointmentDate.DayOfWeek;

            var schedules = await appointmentRepository.GetDoctorSchedulesAsync(
                doctorId,
                dayOfWeek);

            if (!schedules.Any())
            {
                return new List<TimeSpan>();
            }

            var appointments = await appointmentRepository.GetDoctorAppointmentsAsync(
                doctorId,
                appointmentDate);

            const int slotDurationMinutes = 30;

            var slots = new List<TimeSpan>();

            foreach (var schedule in schedules)
            {
                var currentTime = schedule.StartTime;

                while (currentTime.Add(
                           TimeSpan.FromMinutes(slotDurationMinutes))
                       <= schedule.EndTime)
                {
                    var slotEndTime = currentTime.Add(
                        TimeSpan.FromMinutes(slotDurationMinutes));

                    bool isBooked = appointments.Any(x =>
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
