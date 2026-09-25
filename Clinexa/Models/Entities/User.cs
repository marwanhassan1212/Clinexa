using Microsoft.AspNetCore.Identity;

namespace Clinexa.Models.Entities
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public Doctor? Doctor { get; set; }

        public ICollection<AuditLog> AuditLogs { get; set; }
            = new List<AuditLog>();


    }
}
