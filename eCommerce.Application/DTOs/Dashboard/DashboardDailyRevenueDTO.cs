namespace eCommerce.Application.DTOs.Dashboard
{
    public class DashboardDailyRevenueDTO
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
}
