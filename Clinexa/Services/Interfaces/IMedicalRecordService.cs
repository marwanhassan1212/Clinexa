using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<MedicalRecord?> GetByIdAsync(int id);

        Task<List<MedicalRecord>> GetAllAsync();

        Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId);

        Task<List<MedicalRecord>> GetByDoctorIdAsync(int doctorId);
        Task<Appointment?> GetAppointmentByIdAsync(int appointmentId);
        Task<List<MedicalRecord>> GetByPatientIdAsync(int patientId);

        Task<bool> CreateAsync(MedicalRecord medicalRecord);
        Task<List<Appointment>> GetAvailableAppointmentsAsync();

        Task<bool> UpdateAsync(MedicalRecord medicalRecord);
    }
}
