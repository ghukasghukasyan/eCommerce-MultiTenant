using static ClientLibrary.Enums.Statuses;

namespace ClientLibrary.Models.Dashboard
{
    public class DashboardRecentOrderDTO
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
