using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _db;

        public RoleRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task AddAsync(Role role)
        {
            await _db.Roles.AddAsync(role);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _db.Roles
                 .AnyAsync(x => x.Name == name);
        }

        public async Task<List<Role>> GetAllAsync()
        {
            return await _db.Roles
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _db.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.RoleId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(Role role)
        {
            _db.Roles.Update(role);
        }
    }
}
