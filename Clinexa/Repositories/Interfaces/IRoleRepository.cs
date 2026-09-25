using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<bool> ExistsByNameAsync(string name);

        Task<List<Role>> GetAllAsync();

        Task<Role?> GetByIdAsync(int id);


    }
}
