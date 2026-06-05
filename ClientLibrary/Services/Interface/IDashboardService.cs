using ClientLibrary.Models.Dashboard;

namespace ClientLibrary.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardStatsDTO?> GetStatsAsync();
    }
}
