using Clinexa.Enums;
using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Patient
{
    public class EditViewModel
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = null!;

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(255)]
        public string? EmergencyContactName { get; set; }

        [StringLength(30)]
        public string? EmergencyContactPhone { get; set; }

        [StringLength(10)]
        public string? BloodType { get; set; }

        [StringLength(2000)]
        public string? Allergies { get; set; }
    }
}
