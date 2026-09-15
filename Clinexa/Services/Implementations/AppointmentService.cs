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
            if(appointment.StartTime >= appointment.EndTime)
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
    }
}
