using Microsoft.AspNetCore.Identity;

namespace Clinexa.Models.Entities
{
    public class Role : IdentityRole<int>
    {
        public string? Description { get; set; }
    }
}
