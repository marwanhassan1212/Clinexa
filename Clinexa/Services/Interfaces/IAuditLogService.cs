using Clinexa.Models.Entities;

namespace Clinexa.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(string action, string entityName, int entityId,
            int userId, string? oldValues = null, string? newValues = null,
            string? ipAddress = null);
        Task<List<AuditLog>> GetAllAsync();

        Task<List<AuditLog>> GetByUserIdAsync(int userId);

        Task<List<string>> GetEntityNamesAsync();

        Task<List<string>> GetActionsAsync();
        Task<(List<AuditLog> Logs, int TotalCount)> FilterAsync(string? search, string? action,
            string? entityName, int? userId, DateTime? dateFrom, DateTime? dateTo,
            int page, int pageSize);
        Task<List<AuditLog>> GetByEntityAsync(string entityName, int entityId);

        Task<List<User>> GetUsersAsync();
        Task<AuditLog?> GetByIdAsync(int id);
    }
}