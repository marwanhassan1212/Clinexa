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

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
