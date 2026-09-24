namespace Clinexa.Models.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        // Key Metrics

        public int TodayAppointments { get; set; }

        public int ActivePatients { get; set; }

        public int ActiveDoctors { get; set; }

        public decimal TodayRevenue { get; set; }


        // Appointment Summary

        public int CompletedAppointments { get; set; }

        public int UpcomingAppointments { get; set; }

        public int CancelledAppointments { get; set; }

        public int NoShowAppointments { get; set; }


        // Billing Summary
        public int UnpaidInvoices { get; set; }

        public decimal OutstandingAmount { get; set; }


        // Today's Appointments
        public List<DashboardAppointmentViewModel> TodayAppointmentsList { get; set; } = new();



        // Attention Items=

        public List<DashboardAlertViewModel> Alerts { get; set; } = new();

    }
}