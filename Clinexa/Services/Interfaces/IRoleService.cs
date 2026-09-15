using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IRoleService
    {
        Task<Role?> GetByIdAsync(int id);

        Task<List<Role>> GetAllAsync();

        Task<bool> CreateAsync(Role role);

        Task<bool> UpdateAsync(Role role);
    }
}
