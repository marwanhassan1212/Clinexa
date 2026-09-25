using Clinexa.Models.Entities;
using Clinexa.Repositories.Implementations;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Clinexa.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<Role> roleManager;
        public UserService(IUserRepository userRepository , UserManager<User> userManager
            , RoleManager<Role> roleManager)
        {
            this.userRepository = userRepository;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public async Task<bool> CreateAsync(
              User user,
              string password,
              int roleId)
        {
            if (await userRepository.ExistsByEmailAsync(user.Email!))
            {
                return false;
            }

            if (await userRepository.ExistsByPhoneNumberAsync(
                    user.PhoneNumber!))
            {
                return false;
            }

            var role = await roleManager.FindByIdAsync(
                roleId.ToString());

            if (role == null)
            {
                return false;
            }

            user.UserName = user.Email;
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            var result = await userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    " | ",
                    result.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            var roleResult = await userManager.AddToRoleAsync(
                user,
                role.Name!);

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return false;
            }

            return true;
        }

     

        public async Task<bool> DeactivateAsync(int id)
        {
            var user = await userManager.FindByIdAsync(
                id.ToString());

            if (user == null)
            {
                return false;
            }

            user.IsActive = false;

            var result = await userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<bool> Activate(int id)
        {
            var user = await userManager.FindByIdAsync(
                id.ToString());

            if (user == null)
            {
                return false;
            }

            user.IsActive = true;

            var result = await userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await userRepository.GetAllAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await userRepository.GetByIdAsync(id);
        }

        public async Task<(List<User> Users, int TotalCount)> FilterAsync(
            string? search,
            int? roleId,
            bool? isActive,
            int page,
            int pageSize)
        {
            return await userRepository.FilterAsync(
                search,
                roleId,
                isActive,
                page,
                pageSize);
        }

        public async Task<bool> UpdateAsync(User user, int roleId)
        {
            var existingUser =
                await userManager.FindByIdAsync(user.Id.ToString());

            if (existingUser == null)
                return false;


            // Make sure old users have the Identity values
            // required by UserManager before updating them.
            if (string.IsNullOrEmpty(existingUser.SecurityStamp))
            {
                existingUser.SecurityStamp = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrEmpty(existingUser.ConcurrencyStamp))
            {
                existingUser.ConcurrencyStamp = Guid.NewGuid().ToString();
            }


            // =========================
            // Validate Email
            // =========================

            var emailExists =
                await userRepository.ExistsByEmailAsync(user.Email!);

            if (emailExists &&
                !string.Equals(
                    existingUser.Email,
                    user.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }


            // =========================
            // Validate Phone
            // =========================

            var phoneExists =
                await userRepository.ExistsByPhoneNumberAsync(
                    user.PhoneNumber!);

            if (phoneExists &&
                existingUser.PhoneNumber != user.PhoneNumber)
            {
                return false;
            }


            // =========================
            // Validate Role
            // =========================

            var role =
                await roleManager.FindByIdAsync(
                    roleId.ToString());

            if (role == null)
                return false;


            // =========================
            // Update User Data
            // =========================

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.UserName = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;

            existingUser.NormalizedEmail =
                user.Email?.ToUpperInvariant();

            existingUser.NormalizedUserName =
                user.Email?.ToUpperInvariant();


            // =========================
            // Save User
            // =========================

            var updateResult =
                await userManager.UpdateAsync(existingUser);

            if (!updateResult.Succeeded)
                return false;


            // =========================
            // Update Role
            // =========================

            var currentRoles =
                await userManager.GetRolesAsync(existingUser);

            if (currentRoles.Any())
            {
                var removeResult =
                    await userManager.RemoveFromRolesAsync(
                        existingUser,
                        currentRoles);

                if (!removeResult.Succeeded)
                    return false;
            }

            var addRoleResult =
                await userManager.AddToRoleAsync(
                    existingUser,
                    role.Name!);

            return addRoleResult.Succeeded;
        }

        public async Task<string?> GetRoleNameAsync(int userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return null;

            var roles = await userManager.GetRolesAsync(user);

            return roles.FirstOrDefault();
        }

        public async Task<int?> GetRoleIdAsync(int userId)
        {
            return await userRepository.GetRoleIdAsync(userId);
        }
        public async Task<Dictionary<int, string?>> GetRoleNamesAsync(
            IEnumerable<int> userIds)
        {
            return await userRepository.GetRoleNamesAsync(userIds);
        }

        public async Task<bool> SetPasswordAsync(int userId, string newPassword)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return false;

            // Identity creates the real password hash.
            var passwordHash =
                userManager.PasswordHasher.HashPassword(
                    user,
                    newPassword);

            user.PasswordHash = passwordHash;

            // Password change should invalidate old authentication/security data.
            user.SecurityStamp = Guid.NewGuid().ToString();

            var result = await userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return false;

            var role = await roleManager.FindByIdAsync(
                roleId.ToString());

            if (role == null)
                return false;

            var currentRoles = await userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult =
                    await userManager.RemoveFromRolesAsync(
                        user,
                        currentRoles);

                if (!removeResult.Succeeded)
                    return false;
            }

            var result = await userManager.AddToRoleAsync(
                user,
                role.Name!);

            return result.Succeeded;
        }
    }
}
