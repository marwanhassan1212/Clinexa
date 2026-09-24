using Clinexa.Models.ViewModels.Dashboard;
using Clinexa.Repositories.Interfaces;
using Clinexa.Services.Interfaces;

namespace Clinexa.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(
            IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var model = new DashboardViewModel
            {
                // Key Metrics

                TodayAppointments =
                    await _dashboardRepository
                        .GetTodayAppointmentsCountAsync(),

                ActivePatients =
                    await _dashboardRepository
                        .GetActivePatientsCountAsync(),

                ActiveDoctors =
                    await _dashboardRepository
                        .GetActiveDoctorsCountAsync(),

                TodayRevenue =
                    await _dashboardRepository
                        .GetTodayRevenueAsync(),


                // Appointment Summary

                CompletedAppointments =
                    await _dashboardRepository
                        .GetCompletedAppointmentsCountAsync(),

                UpcomingAppointments =
                    await _dashboardRepository
                        .GetUpcomingAppointmentsCountAsync(),

                CancelledAppointments =
                    await _dashboardRepository
                        .GetCancelledAppointmentsCountAsync(),

                NoShowAppointments =
                    await _dashboardRepository
                        .GetNoShowAppointmentsCountAsync(),


                // Billing Summary

                UnpaidInvoices =
                    await _dashboardRepository
                        .GetUnpaidInvoicesCountAsync(),

                OutstandingAmount =
                    await _dashboardRepository
                        .GetOutstandingAmountAsync(),


                // Today's Appointments

                TodayAppointmentsList =
                    await _dashboardRepository
                        .GetTodayAppointmentsAsync(),

                Alerts = await _dashboardRepository.GetAlertsAsync()

            };

            return model;
        }
    }
}
