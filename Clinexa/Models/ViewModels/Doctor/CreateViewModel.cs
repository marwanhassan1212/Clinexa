using System.ComponentModel.DataAnnotations;

namespace Clinexa.Models.ViewModels.Doctor
{
    public class CreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a user.")]
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a speciality.")]
        public int SpecialityId { get; set; }

        [Range(typeof(decimal), "0", "1000000",
            ErrorMessage = "Consultation fee must be between 0 and 1,000,000.")]
        public decimal ConsultationFee { get; set; }
    }
}
