namespace Clinexa.Models.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public Role Role { get; set; } = null!;
        public int RoleId { get; set; }
        public Doctor? Doctor { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();


    }
}
