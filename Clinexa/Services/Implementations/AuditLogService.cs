using Clinexa.Models.Entities;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogService(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        public async Task LogAsync(string action, string entityName, int entityId,
            int userId, string? oldValues = null, string? newValues = null, string? ipAddress = null)

        {
            var auditLog = new AuditLog
            {
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                UserId = userId,
                OldValues = oldValues,
                NewValues = newValues,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(auditLog);
            await _auditLogRepository.SaveChangesAsync();
        }

        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _auditLogRepository.GetAllAsync();
        }

        public async Task<List<AuditLog>> GetByUserIdAsync(int userId)
        {
            return await _auditLogRepository.GetByUserIdAsync(userId);
        }

        public async Task<List<AuditLog>> GetByEntityAsync(string entityName, int entityId)
        {
            return await _auditLogRepository.GetByEntityAsync(
                entityName,
                entityId);
        }
    }
}
