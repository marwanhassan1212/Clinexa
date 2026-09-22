using Clinexa.Enums;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Implementations;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository patientRepository;
        public PatientService(IPatientRepository patientRepository)
        {
            this.patientRepository = patientRepository;
        }
        public async Task<bool> CreateAsync(Patient patient)
        {
            bool phoneExist = await patientRepository.ExistsByPhoneAsync(patient.PhoneNumber);
            if(phoneExist)
            {
                return false;
            }
            else
            {
                patient.CreatedAt = DateTime.UtcNow;
                patient.IsActive = true;
                await patientRepository.AddAsync(patient);
                await patientRepository.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var patientExists = await patientRepository.GetByIdAsync(id);
            if (patientExists == null)
            {
                return false;
            }
            else
            {
                patientExists.IsActive = false;
                patientRepository.Update(patientExists);
                await patientRepository.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<Patient>> GetAllAsync()
        {
            return await patientRepository.GetAllAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await patientRepository.GetByIdAsync(id);
        }

        public async Task<List<Patient>> SearchAsync(string searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm))
            {
                return await patientRepository.GetAllAsync();
            }
            else
            {
                return await patientRepository.SearchAsync(searchTerm);
            }
        }

        public async Task<bool> UpdateAsync(Patient patient)
        {
            var patientExists = await patientRepository.GetByIdAsync(patient.PatientId);
            if(patientExists == null)
            {
                return false;
            }
            else
            {
                patientRepository.Update(patient);
                await patientRepository.SaveChangesAsync();
                return true;
            }

        }

        public async Task<bool> ActivateAsync(int id)
        {
            var patient = await patientRepository.GetByIdAsync(id);

            if (patient == null)
            {
                return false;
            }

            patient.IsActive = true;

            patientRepository.Update(patient);
            await patientRepository.SaveChangesAsync();

            return true;
        }

        public async Task<(List<Patient> Patients, int TotalCount)> FilterAsync(string? search,
                  Gender? gender, string? bloodType, bool? isActive, int page, int pageSize)
        {
            return await patientRepository.FilterAsync(
                search,
                gender,
                bloodType,
                isActive,
                page,
                pageSize);
        }

    }
}
