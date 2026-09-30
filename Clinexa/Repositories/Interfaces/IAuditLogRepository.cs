using Clinexa.Models.Entities;

namespace Clinexa.Repositories.Interfaces
{
    public interface IAuditLogRepository
    {
        Task AddAsync(AuditLog auditLog);

        Task<List<AuditLog>> GetAllAsync();

        Task<List<AuditLog>> GetByUserIdAsync(int userId);
        Task<List<string>> GetEntityNamesAsync();

        Task<List<string>> GetActionsAsync();
        Task<(List<AuditLog> Logs, int TotalCount)> FilterAsync(string? search, string? action,
            string? entityName, int? userId, DateTime? dateFrom, DateTime? dateTo, int page,
            int pageSize);
        Task<List<AuditLog>> GetByEntityAsync(string entityName, int entityId);

        Task<List<User>> GetUsersAsync();
        Task<AuditLog?> GetByIdAsync(int id);

        Task SaveChangesAsync();
    }
}
