using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(int id);

        Task<List<Role>> GetAllAsync();

        Task<bool> ExistsByNameAsync(string name);

        Task AddAsync(Role role);

        void Update(Role role);

        Task SaveChangesAsync();
    }
}
