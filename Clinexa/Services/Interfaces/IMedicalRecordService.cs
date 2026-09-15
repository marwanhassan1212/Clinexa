using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IMedicalRecordService
    {
        Task<MedicalRecord?> GetByIdAsync(int id);

        Task<List<MedicalRecord>> GetAllAsync();

        Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId);

        Task<List<MedicalRecord>> GetByDoctorIdAsync(int doctorId);

        Task<List<MedicalRecord>> GetByPatientIdAsync(int patientId);

        Task<bool> CreateAsync(MedicalRecord medicalRecord);

        Task<bool> UpdateAsync(MedicalRecord medicalRecord);
    }
}
