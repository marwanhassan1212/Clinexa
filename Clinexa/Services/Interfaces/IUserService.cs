using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(int id);

        Task<List<User>> GetAllAsync();

        Task<bool> CreateAsync(User user , string password , int roleId);

        Task<bool> UpdateAsync(User user , int roleId);

        Task<string?> GetRoleNameAsync(int userId);
        Task<int?> GetRoleIdAsync(int userId);
        Task<Dictionary<int, string?>> GetRoleNamesAsync(IEnumerable<int> userIds);

        Task<bool> SetPasswordAsync(int userId, string newPassword);

        Task<bool> AssignRoleAsync(int userId, int roleId);

        Task<(List<User> Users, int TotalCount)> FilterAsync(string? search, int? roleId,
                bool? isActive, int page, int pageSize);
        Task<bool> DeactivateAsync(int id);
        Task<bool> Activate(int id);
    }
}
