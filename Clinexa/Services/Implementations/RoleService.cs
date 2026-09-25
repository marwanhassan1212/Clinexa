using Clinexa.Models.Entities;
using Clinexa.Repositories.Implementations;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Clinexa.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository roleRepository;
        private readonly RoleManager<Role> roleManager;

        public RoleService(IRoleRepository roleRepository , RoleManager<Role> roleManager)
        {
            this.roleRepository = roleRepository;
            this.roleManager = roleManager;
        }

        public async Task<bool> CreateAsync(Role role)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
                return false;

            var exists = await roleRepository.ExistsByNameAsync(role.Name);

            if (exists)
                return false;

            var result = await roleManager.CreateAsync(role);

            return result.Succeeded;
        }

        public async Task<List<Role>> GetAllAsync()
        {
            return await roleRepository.GetAllAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await roleRepository.GetByIdAsync(id);
        }

        public async Task<bool> UpdateAsync(Role role)
        {
            if (string.IsNullOrWhiteSpace(role.Name))
                return false;

            var existingRole = await roleManager.FindByIdAsync(
                role.Id.ToString());

            if (existingRole == null)
                return false;

            var exists = await roleRepository.ExistsByNameAsync(role.Name);

            if (exists &&
                !string.Equals(
                    existingRole.Name,
                    role.Name,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;

            var result = await roleManager.UpdateAsync(existingRole);

            return result.Succeeded;
        }
    }
}
