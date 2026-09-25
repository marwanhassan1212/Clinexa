using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);

        Task<List<AuditLog>> GetAllAsync();

        Task<List<AuditLog>> GetByUserIdAsync(int userId);

        Task<List<AuditLog>> GetByEntityAsync(
            string entityName,
            int entityId);

        Task SaveChangesAsync();
    }
}
