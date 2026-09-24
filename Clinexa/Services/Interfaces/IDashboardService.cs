using Clinexa.Models.ViewModels.Dashboard;

namespace Clinexa.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardAsync();
    }
}
