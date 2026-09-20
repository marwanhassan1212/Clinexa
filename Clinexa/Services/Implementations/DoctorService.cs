using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

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

            if (doctorExists == null)
                return false;

            if (doctor.ConsultationFee < 0)
                return false;

            doctorExists.SpecialityId = doctor.SpecialityId;
            doctorExists.ConsultationFee = doctor.ConsultationFee;

            doctorRepository.UpdateAsync(doctorExists);

            await doctorRepository.SaveChangesAsync();

            return true;
        }

        public async Task<(List<Doctor> Doctors, int TotalCount)> FilterAsync(
                  string? search,
                  int? specialityId,
                  bool? isActive,
                  int page,
                  int pageSize)
        {
            return await doctorRepository.FilterAsync(
                search,
                specialityId,
                isActive,
                page,
                pageSize);
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var doctorExists = await doctorRepository.GetByIdAsync(id);
            if(doctorExists == null)
            {
                return false;
            }
            doctorExists.IsActive = true;
            doctorRepository.UpdateAsync(doctorExists);
            await doctorRepository.SaveChangesAsync();
            return true;

        }

        public async Task<List<User>> GetAvailableUsersAsync()
        {
            return await doctorRepository.GetAvailableUsersAsync();
        }
    }
}
