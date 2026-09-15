namespace Clinexa.Models.Entities
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string Action { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public int EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public User User { get; set; } = null!;
        public int UserId { get; set; }


    }
}
