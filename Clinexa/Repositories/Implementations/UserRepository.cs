using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _db.Users
                .AsNoTracking()
                .AnyAsync(x => x.Email == email);
        }

        public async Task<bool> ExistsByPhoneNumberAsync(string phoneNumber)
        {
            return await _db.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _db.Users
                  .AsNoTracking()
                  .OrderBy(x => x.FirstName)
                  .ThenBy(x => x.LastName)
                  .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.Users
                 .AsNoTracking()
                 .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Update(User user)
        {
            _db.Users.Update(user);
        }

        public async Task<(List<User> Users, int TotalCount)> FilterAsync(string? search, int? roleId,
               bool? isActive, int page, int pageSize)
        {
            var query = _db.Users
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    (x.FirstName + " " + x.LastName).Contains(search) ||
                    x.Email!.Contains(search) ||
                    x.PhoneNumber!.Contains(search));
            }

            if (roleId.HasValue)
            {
                query = query.Where(user =>
                    _db.UserRoles.Any(userRole =>
                        userRole.UserId == user.Id &&
                        userRole.RoleId == roleId.Value));
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }

        public async Task<int?> GetRoleIdAsync(int userId)
        {
            return await _db.UserRoles
                .Where(x => x.UserId == userId)
                .Select(x => (int?)x.RoleId)
                .FirstOrDefaultAsync();
        }

        public async Task<Dictionary<int, string?>> GetRoleNamesAsync(
     IEnumerable<int> userIds)
        {
            var ids = userIds.ToList();

            var roleNames = await _db.UserRoles
                .Where(ur => ids.Contains(ur.UserId))
                .Join(
                    _db.Roles,
                    ur => ur.RoleId,
                    role => role.Id,
                    (ur, role) => new
                    {
                        ur.UserId,
                        RoleName = role.Name
                    })
                .ToListAsync();

            return roleNames.ToDictionary(
                x => x.UserId,
                x => x.RoleName);
        }
    }
}
