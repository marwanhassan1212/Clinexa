using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository doctorRepository;
        public DoctorService(IDoctorRepository doctorRepository)
        {
            this.doctorRepository = doctorRepository;
        }

        public async Task<bool> CreateAsync(Doctor doctor)
        {
            bool userExist = await doctorRepository.ExistsByUserId(doctor.UserId);
            if(userExist)
            {
                return false;
            }
            bool specialiyExists = await doctorRepository.ExistBySpecialityId(doctor.SpecialityId);
            if(!specialiyExists)
            {
                return false;
            }
            if(doctor.ConsultationFee < 0)
            {
                return false;
            }
            doctor.CreatedAt = DateTime.UtcNow;
            doctor.IsActive = true;
            await doctorRepository.AddAsync(doctor);
            await doctorRepository.SaveChangesAsync();
            return true;
            
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var doctorExists = await doctorRepository.GetByIdAsync(id);
            if(doctorExists == null)
            {
                return false;
            }
            else
            {
                doctorExists.IsActive = false;
                doctorRepository.UpdateAsync(doctorExists);
                await doctorRepository.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<Doctor>> GetAllAsync()
        {
            return await doctorRepository.GetAllAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await doctorRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(Doctor doctor)
        {
            var doctorExists = await doctorRepository.GetByIdAsync(doctor.DoctorId);
            if(doctorExists == null)
            {
                return false;
            }
            if(doctor.ConsultationFee < 0)
            {
                return false;
            }
            doctorRepository.UpdateAsync(doctor);
            await doctorRepository.SaveChangesAsync();
            return true;
        }
    }
}
