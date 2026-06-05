using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Dashboard
{
    public class DashboardRecentOrderDTO
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
