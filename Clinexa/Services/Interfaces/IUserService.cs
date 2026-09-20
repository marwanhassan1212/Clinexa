using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);

        Task<List<User>> GetAllAsync();

        Task<bool> CreateAsync(User user);

        Task<bool> UpdateAsync(User user);
        Task<(List<User> Users, int TotalCount)> FilterAsync(
                string? search,
                int? roleId,
                bool? isActive,
                int page,
                int pageSize);

        Task<bool> DeactivateAsync(int id);
        Task<bool> Activate(int id);
    }
}
