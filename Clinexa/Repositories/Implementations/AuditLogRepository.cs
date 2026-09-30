using Clinexa.Data;
using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Clinexa.Repositories.Implementations
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _db;

        public AuditLogRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(AuditLog auditLog)
        {
            await _db.AuditLogs.AddAsync(auditLog);
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _db.AuditLogs
                .AsNoTracking()
                .Include(x => x.User)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByUserIdAsync(int userId)
        {
            return await _db.AuditLogs
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetByEntityAsync(
            string entityName,
            int entityId)
        {
            return await _db.AuditLogs
                .AsNoTracking()
                .Include(x => x.User)
                .Where(x =>
                    x.EntityName == entityName &&
                    x.EntityId == entityId)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _db.AuditLogs
                .AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.AuditLogId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<(List<AuditLog> Logs, int TotalCount)> FilterAsync(
            string? search,
            string? action,
            string? entityName,
            int? userId,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page,
            int pageSize)
        {
            IQueryable<AuditLog> query = _db.AuditLogs
                .AsNoTracking()
                .Include(x => x.User);


            // -----------------------------------------------------
            // Search
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                if (int.TryParse(search, out int entityId))
                {
                    query = query.Where(x =>
                        x.EntityName.Contains(search) ||
                        x.Action.Contains(search) ||
                        (x.IpAddress != null &&
                         x.IpAddress.Contains(search)) ||
                        x.EntityId == entityId);
                }
                else
                {
                    query = query.Where(x =>
                        x.EntityName.Contains(search) ||
                        x.Action.Contains(search) ||
                        (x.IpAddress != null &&
                         x.IpAddress.Contains(search)));
                }
            }


            // -----------------------------------------------------
            // Action
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(x =>
                    x.Action == action);
            }


            // -----------------------------------------------------
            // Entity
            // -----------------------------------------------------

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                query = query.Where(x =>
                    x.EntityName == entityName);
            }


            // -----------------------------------------------------
            // User
            // -----------------------------------------------------

            if (userId.HasValue)
            {
                query = query.Where(x =>
                    x.UserId == userId.Value);
            }


            // -----------------------------------------------------
            // Date From
            // -----------------------------------------------------

            if (dateFrom.HasValue)
            {
                var fromDate = dateFrom.Value.Date;

                query = query.Where(x =>
                    x.Timestamp >= fromDate);
            }


            // -----------------------------------------------------
            // Date To
            // -----------------------------------------------------

            if (dateTo.HasValue)
            {
                var toDate = dateTo.Value.Date.AddDays(1);

                query = query.Where(x =>
                    x.Timestamp < toDate);
            }


            // -----------------------------------------------------
            // Total Count
            // -----------------------------------------------------

            var totalCount = await query.CountAsync();


            // -----------------------------------------------------
            // Pagination
            // -----------------------------------------------------

            var logs = await query
                .OrderByDescending(x => x.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return (logs, totalCount);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _db.Users
                .AsNoTracking()
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToListAsync();
        }

        public async Task<List<string>> GetEntityNamesAsync()
        {
            return await _db.AuditLogs
                .AsNoTracking()
                .Select(x => x.EntityName)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<List<string>> GetActionsAsync()
        {
            return await _db.AuditLogs
                .AsNoTracking()
                .Select(x => x.Action)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }
    }
}