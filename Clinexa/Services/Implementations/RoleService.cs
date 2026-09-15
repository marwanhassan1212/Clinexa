using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            this.roleRepository = roleRepository;
        }

        public async Task<bool> CreateAsync(Role role)
        {
            bool nameExists = await roleRepository
               .ExistsByNameAsync(role.Name);

            if (nameExists)
            {
                return false;
            }

            await roleRepository.AddAsync(role);
            await roleRepository.SaveChangesAsync();

            return true;
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
            var roleExists = await roleRepository
                .GetByIdAsync(role.RoleId);

            if (roleExists == null)
            {
                return false;
            }

            bool nameExists = await roleRepository
                .ExistsByNameAsync(role.Name);

            if (nameExists && roleExists.Name != role.Name)
            {
                return false;
            }

            roleRepository.Update(role);

            await roleRepository.SaveChangesAsync();

            return true;
        }
    }
}
