using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);

        Task<List<User>> GetAllAsync();

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);
        Task<(List<User> Users, int TotalCount)> FilterAsync(string? search, int? roleId,
                bool? isActive,
                int page,
                int pageSize);
        Task AddAsync(User user);

        void Update(User user);
        Task<int?> GetRoleIdAsync(int userId);

        Task<Dictionary<int, string>> GetRoleNamesAsync(IEnumerable<int> userIds);


        Task SaveChangesAsync();
    }
}
