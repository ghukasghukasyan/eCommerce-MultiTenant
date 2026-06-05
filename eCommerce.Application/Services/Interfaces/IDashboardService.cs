using eCommerce.Application.DTOs.Dashboard;

namespace eCommerce.Application.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDTO> GetStatsAsync();
    }
}
