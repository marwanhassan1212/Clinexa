using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class MedicalRecordService : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository medicalRecordRepository;
        public MedicalRecordService(IMedicalRecordRepository medicalRecordRepository)
        {
            this.medicalRecordRepository = medicalRecordRepository;
        }
        public async Task<bool> CreateAsync(MedicalRecord medicalRecord)
        {
            bool appointmentExists =
                await medicalRecordRepository
                    .AppointmentExistsAsync(
                        medicalRecord.AppointmentId);
            if(!appointmentExists)
            {
                return false;
            }

            bool doctorExists =
           await medicalRecordRepository
               .DoctorExistsAsync(
                   medicalRecord.DoctorId);

            if (!doctorExists)
            {
                return false;
            }

            bool doctorAssigned =
               await medicalRecordRepository
                   .IsDoctorAssignedToAppointmentAsync(
                       medicalRecord.AppointmentId,
                       medicalRecord.DoctorId);

            if (!doctorAssigned)
            {
                return false;
            }


            bool recordExists =
               await medicalRecordRepository
                   .ExistsForAppointmentAsync(
                       medicalRecord.AppointmentId);

            if (recordExists)
            {
                return false;
            }

            bool patientAssigned =
            await medicalRecordRepository
                .IsPatientAssignedToAppointmentAsync(
                    medicalRecord.AppointmentId,
                    medicalRecord.PatientId);

            if (!patientAssigned)
            {
                return false;
            }

            await medicalRecordRepository
               .AddAsync(medicalRecord);

            
            await medicalRecordRepository
                .SaveChangesAsync();

            return true;

        }

        public async Task<List<MedicalRecord>> GetAllAsync()
        {
            return await medicalRecordRepository.GetAllAsync();
        }

        public async Task<MedicalRecord?> GetByAppointmentIdAsync(int appointmentId)
        {
            return await medicalRecordRepository.GetByAppointmentIdAsync(appointmentId);
        }

        public async Task<List<MedicalRecord>> GetByDoctorIdAsync(int doctorId)
        {
            return await medicalRecordRepository.GetByDoctorIdAsync(doctorId);
        }

        public async Task<MedicalRecord?> GetByIdAsync(int id)
        {
            return await medicalRecordRepository.GetByIdAsync(id);
        }

        public async Task<List<MedicalRecord>> GetByPatientIdAsync(int patientId)
        {
            return await medicalRecordRepository.GetByPatientIdAsync(patientId);
        }

        public async Task<bool> UpdateAsync(MedicalRecord medicalRecord)
        {
            
            var medicalRecordExists =
                await medicalRecordRepository
                    .GetByIdAsync(
                        medicalRecord.MedicalRecordId);

            if (medicalRecordExists == null)
            {
                return false;
            }

          
            bool appointmentExists =
                await medicalRecordRepository
                    .AppointmentExistsAsync(
                        medicalRecord.AppointmentId);

            if (!appointmentExists)
            {
                return false;
            }

           
            bool doctorExists =
                await medicalRecordRepository
                    .DoctorExistsAsync(
                        medicalRecord.DoctorId);

            if (!doctorExists)
            {
                return false;
            }

           
            bool doctorAssigned =
                await medicalRecordRepository
                    .IsDoctorAssignedToAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.DoctorId);

            if (!doctorAssigned)
            {
                return false;
            }

            bool patientAssigned =
                await medicalRecordRepository
                    .IsPatientAssignedToAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.PatientId);

            if (!patientAssigned)
            {
                return false;
            }


            bool duplicate =
                await medicalRecordRepository
                    .ExistsForAppointmentAsync(
                        medicalRecord.AppointmentId,
                        medicalRecord.MedicalRecordId);

            if (duplicate)
            {
                return false;
            }

            
            medicalRecordRepository.Update(
                medicalRecord);

           
            await medicalRecordRepository
                .SaveChangesAsync();

            return true;
        }

        public async Task<List<Appointment>> GetAvailableAppointmentsAsync()
        {
            return await medicalRecordRepository
                .GetAvailableAppointmentsAsync();
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(
         int appointmentId)
        {
            return await medicalRecordRepository
                .GetAppointmentByIdAsync(
                    appointmentId);
        }
    }
}
