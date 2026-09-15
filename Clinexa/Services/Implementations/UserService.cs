using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<bool> CreateAsync(User user)
        {
            bool emailExists = await userRepository.ExistsByEmailAsync(user.Email);
            if(emailExists)
            {
                return false;
            }
            bool phoneExists = await userRepository.ExistsByPhoneNumberAsync(user.PhoneNumber);
            if(phoneExists)
            {
                return false;
            }
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;
            await userRepository.AddAsync(user);
            await userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            var userExists = await userRepository.GetByIdAsync(id);

            if (userExists == null)
            {
                return false;
            }

            userExists.IsActive = false;

            userRepository.Update(userExists);

            await userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await userRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(User user)
        {
            var userExists = await userRepository.GetByIdAsync(user.UserId);
            if(userExists == null)
            {
                return false;
            }
            bool emailExists = await userRepository
              .ExistsByEmailAsync(user.Email);

            if (emailExists && userExists.Email != user.Email)
            {
                return false;
            }

            bool phoneExists = await userRepository
                .ExistsByPhoneNumberAsync(user.PhoneNumber);

            if (phoneExists && userExists.PhoneNumber != user.PhoneNumber)
            {
                return false;
            }

            userRepository.Update(user);
            await userRepository.SaveChangesAsync();
            return true;
        }
    }
}
