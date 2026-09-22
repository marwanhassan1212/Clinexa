using Clinexa.Models.Entities;

namespace Clinexa.Models.ViewModels.Appointment
{
    public class AppointmentDetailsViewModel
    {
        public Entities.Appointment Appointment { get; set; } = null!;

        public Entities.Invoice? Invoice { get; set; }
    }
}