using Clinexa.Models.Entities;

namespace Clinexa.Models.ViewModels.AuditLog
{
    public class AuditLogFilterViewModel
    {
        public string? Search { get; set; }

        public string? Action { get; set; }

        public string? EntityName { get; set; }

        public int? UserId { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public int TotalCount { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling(
                TotalCount / (double)PageSize);

        public List<Entities.AuditLog> AuditLogs { get; set; } = new();

        public List<Entities.User> Users { get; set; } = new();

        public List<string> EntityNames { get; set; } = new();

        public List<string> Actions { get; set; } = new();
    }
}