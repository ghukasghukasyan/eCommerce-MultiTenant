namespace ClientLibrary.Models.Dashboard
{
    public class DashboardStatsDTO
    {
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public int TotalUsers { get; set; }
        public int PendingOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int ApprovedInfluencers { get; set; }
        public List<DashboardRecentOrderDTO> RecentOrders { get; set; } = [];
        public List<DashboardDailyRevenueDTO> WeeklyRevenue { get; set; } = [];
    }
}
