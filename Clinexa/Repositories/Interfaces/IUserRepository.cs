using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);

        Task<List<User>> GetAllAsync();

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber);

        Task AddAsync(User user);

        void Update(User user);

        Task SaveChangesAsync();
    }
}
