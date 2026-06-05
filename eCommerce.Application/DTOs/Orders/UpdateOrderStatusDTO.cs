using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Orders
{
    public class UpdateOrderStatusDTO
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
    }
}
