using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Clinexa.Repositories.Implementations
{
    public class MedicalRecordRepository : IMedicalRecordRepository
    {
        private readonly AppDbContext _db;
        public MedicalRecordRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(MedicalRecord medicalRecord)
        {
            await _db.MedicalRecords.AddAsync(medicalRecord);
        }

        public async Task<bool> AppointmentExistsAsync(int appointmentId)
        {
            return await _db.Appointments.AnyAsync(x => x.AppointmentId == appointmentId);
        }

        public async Task<bool> DoctorExistsAsync(int doctorId)
        {
            return await _db.Doctors.AnyAsync(x => x.DoctorId == doctorId);
        }

        public async Task<bool> ExistsForAppointmentAsync(int appointmentId, int? excludedMedicalRecordId = null)
        {
            return await _db.MedicalRecords
                .AnyAsync(x =>
                    x.AppointmentId == appointmentId &&
                    (!excludedMedicalRecordId.HasValue ||
                     x.MedicalRecordId != excludedMedicalRecordId.Value));
        }

        public async Task<List<MedicalRecord>> GetAllAsync()
        {
            return await _db.MedicalRecords
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                .Include(x => x.Appointment)
                .OrderByDescending(x => x.MedicalRecordId)
                .ToListAsync();
        }

        public async Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await _db.MedicalRecords
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);
        }

        public async Task<List<MedicalRecord>> GetByDoctorIdAsync(int doctorId)
        {
            return await _db.MedicalRecords
                .Where(x => x.DoctorId == doctorId)
                .OrderByDescending(x => x.MedicalRecordId)
                .ToListAsync();
        }


        public async Task<MedicalRecord?> GetByIdAsync(int id)
        {
            return await _db.MedicalRecords
                    .Include(x => x.Patient)
                    .Include(x => x.Doctor)
                        .ThenInclude(x => x.User)
                    .Include(x => x.Appointment)
                    .FirstOrDefaultAsync(
                        x => x.MedicalRecordId == id);
        }

        public async Task<List<MedicalRecord>> GetByPatientIdAsync(int patientId)
        {
            return await _db.MedicalRecords.Where(x => x.PatientId == patientId)
                .OrderByDescending(x => x.MedicalRecordId)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(MedicalRecord medicalRecord)
        {
            _db.MedicalRecords.Update(medicalRecord);
        }

        public async Task<bool> IsDoctorAssignedToAppointmentAsync(int appointmentId, int doctorId)
        {
            return await _db.Appointments
                .AnyAsync(x =>
                    x.AppointmentId == appointmentId &&
                    x.DoctorId == doctorId);
        }

        public async Task<List<Appointment>> GetAvailableAppointmentsAsync()
        {
            return await _db.Appointments
                .AsNoTracking()
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                    .ThenInclude(x => x.User)
                .Where(x =>
                    x.MedicalRecord == null &&
                    x.AppointmentDate <= DateTime.Today)
                .OrderByDescending(x => x.AppointmentDate)
                .ThenByDescending(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _db.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.AppointmentId == appointmentId);
        }
    }
}
