using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IMedicalRecordRepository
    {
        Task<MedicalRecord?> GetByIdAsync(int id);

        Task<List<MedicalRecord>> GetAllAsync();

        Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId);

        Task<List<MedicalRecord>> GetByDoctorIdAsync(int doctorId);
        Task<bool> IsPatientAssignedToAppointmentAsync(int appointmentId, int patientId);

        Task<List<MedicalRecord>> GetByPatientIdAsync(int patientId);

        Task<bool> AppointmentExistsAsync(int appointmentId);

        Task<bool> DoctorExistsAsync(int doctorId);

        Task<bool> ExistsForAppointmentAsync(
            int appointmentId,
            int? excludedMedicalRecordId = null);

        Task AddAsync(MedicalRecord medicalRecord);

        void Update(MedicalRecord medicalRecord);
        Task<List<Appointment>> GetAvailableAppointmentsAsync();
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);


        Task SaveChangesAsync();

        Task<bool> IsDoctorAssignedToAppointmentAsync(int appointmentId, int doctorId);


    }
}
