using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityName, int entityId, int userId,
           string? oldValues = null, string? newValues = null, string? ipAddress = null);
        Task<List<AuditLog>> GetAllAsync();

        Task<List<AuditLog>> GetByUserIdAsync(int userId);

        Task<List<AuditLog>> GetByEntityAsync(string entityName, int entityId);
    }
}
