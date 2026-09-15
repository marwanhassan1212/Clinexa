using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface ISpecialityRepository
    {
        Task<Speciality?> GetByIdAsync(int id);
        Task<List<Speciality>> GetAllAsync();
        Task<bool> ExistsByNameAsync(string name);
        Task AddAsync(Speciality speciality);
        void Update(Speciality speciality);
        Task SaveChangesAsync();
    }
}
