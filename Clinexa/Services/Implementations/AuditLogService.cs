using Clinexa.Models.Entities;
using Clinexa.Repositories.Implementations;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IDateTimeService dateTimeService;

        public AuditLogService(IAuditLogRepository auditLogRepository , IDateTimeService dateTimeService)

        {
            _auditLogRepository = auditLogRepository;
            this.dateTimeService = dateTimeService;
        }

        public async Task LogAsync(
            string action,
            string entityName,
            int entityId,
            int userId,
            string? oldValues = null,
            string? newValues = null,
            string? ipAddress = null)
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

                Timestamp = dateTimeService.Now
            };

            await _auditLogRepository.AddAsync(auditLog);

            await _auditLogRepository.SaveChangesAsync();
        }


        public async Task<List<AuditLog>> GetAllAsync()
        {
            return await _auditLogRepository
                .GetAllAsync();
        }

        public async Task<List<AuditLog>> GetByUserIdAsync(
            int userId)
        {
            return await _auditLogRepository
                .GetByUserIdAsync(userId);
        }

        public async Task<List<AuditLog>> GetByEntityAsync(
            string entityName,
            int entityId)
        {
            return await _auditLogRepository
                .GetByEntityAsync(
                    entityName,
                    entityId);
        }

        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _auditLogRepository
                .GetByIdAsync(id);
        }

        public async Task<(List<AuditLog> Logs, int TotalCount)> FilterAsync(string? search, string? action,
             string? entityName, int? userId, DateTime? dateFrom, DateTime? dateTo, int page, int pageSize)

        {
            return await _auditLogRepository
                .FilterAsync(search, action, entityName, userId, dateFrom, dateTo, page,
                    pageSize);

        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _auditLogRepository.GetUsersAsync();
        }

        public async Task<List<string>> GetEntityNamesAsync()
        {
            return await _auditLogRepository.GetEntityNamesAsync();
        }

        public async Task<List<string>> GetActionsAsync()
        {
            return await _auditLogRepository.GetActionsAsync();
        }
    }
}