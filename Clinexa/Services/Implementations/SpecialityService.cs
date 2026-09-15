using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class SpecialityService : ISpecialityService
    {
        private readonly ISpecialityRepository specialityRepository;
        public SpecialityService(ISpecialityRepository specialityRepository)
        {
            this.specialityRepository = specialityRepository;
        }
        public async Task<bool> CreateAsync(Speciality speciality)
        {
            bool nameExists = await specialityRepository.ExistsByNameAsync(speciality.Name);
            if(nameExists)
            {
                return false;
            }
            else
            {
                speciality.IsActive = true;
                await specialityRepository.AddAsync(speciality);
                await specialityRepository.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var specialityExists = await specialityRepository.GetByIdAsync(id);
            if(specialityExists == null)
            {
                return false;
            }
            else
            {
                specialityExists.IsActive = false;
                specialityRepository.Update(specialityExists);
                await specialityRepository.SaveChangesAsync();
                return true;
                
            }
        }

        public async Task<List<Speciality>> GetAllAsync()
        {
            return await specialityRepository.GetAllAsync();
        }

        public async Task<Speciality?> GetByIdAsync(int id)
        {
            return await specialityRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(Speciality speciality)
        {
            var specialityExists = await specialityRepository.GetByIdAsync(speciality.SpecialityId);
            if(specialityExists == null)
            {
                return false;
            }
            bool nameExists = await specialityRepository.ExistsByNameAsync(speciality.Name);
            if(nameExists && specialityExists.Name != speciality.Name)
            {
                return false;
            }
            else
            {
                specialityRepository.Update(speciality);

                await specialityRepository.SaveChangesAsync();

                return true;
            }
        }
    }
}
