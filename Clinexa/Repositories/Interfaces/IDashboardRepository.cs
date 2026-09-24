using Clinexa.Models.ViewModels.Dashboard;

namespace Clinexa.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<int> GetTodayAppointmentsCountAsync();

        Task<int> GetActivePatientsCountAsync();

        Task<int> GetActiveDoctorsCountAsync();

        Task<decimal> GetTodayRevenueAsync();

        Task<int> GetCompletedAppointmentsCountAsync();

        Task<int> GetUpcomingAppointmentsCountAsync();

        Task<int> GetCancelledAppointmentsCountAsync();

        Task<int> GetNoShowAppointmentsCountAsync();

        Task<int> GetUnpaidInvoicesCountAsync();

        Task<decimal> GetOutstandingAmountAsync();

        Task<List<DashboardAppointmentViewModel>> GetTodayAppointmentsAsync();

        Task<List<DashboardAlertViewModel>> GetAlertsAsync();
    }
}
