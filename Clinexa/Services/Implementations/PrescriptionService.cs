using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository prescriptionRepository;
        public PrescriptionService(IPrescriptionRepository prescriptionRepository)
        {
            this.prescriptionRepository = prescriptionRepository;
        }
        public async Task<bool> CreateAsync(Prescription prescription)
        {
            bool medicalRecordExists =
                await prescriptionRepository
                    .MedicalRecordExistsAsync(
                        prescription.MedicalRecordId);

            if (!medicalRecordExists)
            {
                return false;
            }

            
            bool prescriptionExists =
                await prescriptionRepository
                    .ExistsForMedicalRecordAsync(
                        prescription.MedicalRecordId);

            if (prescriptionExists)
            {
                return false;
            }

            
            if (prescription.PrescriptionDate >
                DateTime.Now)
            {
                return false;
            }

            
            await prescriptionRepository
                .AddAsync(prescription);

            
            await prescriptionRepository
                .SaveChangesAsync();

            return true;
        }

        public async Task<List<Prescription>> GetAllAsync()
        {
            return await prescriptionRepository.GetAllAsync();
        }

        public async Task<Prescription?> GetByIdAsync(int id)
        {
            return await prescriptionRepository.GetByIdAsync(id);
        }

        public Task<Prescription?> GetByMedicalRecordIdAsync(int medicalRecordId)
        {
            return prescriptionRepository.GetByMedicalRecordIdAsync(medicalRecordId);
        }

        public async Task<bool> UpdateAsync(Prescription prescription)
        {
            var prescriptionExists =
                await prescriptionRepository
                    .GetByIdAsync(
                        prescription.PrescriptionId);

            if (prescriptionExists == null)
            {
                return false;
            }

            bool medicalRecordExists =
               await prescriptionRepository
                   .MedicalRecordExistsAsync(
                       prescription.MedicalRecordId);

            if (!medicalRecordExists)
            {
                return false;
            }

            bool duplicate =
               await prescriptionRepository
                   .ExistsForMedicalRecordAsync(
                       prescription.MedicalRecordId,
                       prescription.PrescriptionId);

            if (duplicate)
            {
                return false;
            }

            if (prescription.PrescriptionDate >
               DateTime.Now)
            {
                return false;
            }
          
            prescriptionRepository
                .Update(prescription);
           
            await prescriptionRepository
                .SaveChangesAsync();

            return true;
        }
    }
}
