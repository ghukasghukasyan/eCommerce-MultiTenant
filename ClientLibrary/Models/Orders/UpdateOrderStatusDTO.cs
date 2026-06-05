using static ClientLibrary.Enums.Statuses;

namespace ClientLibrary.Models.Orders
{
    public class UpdateOrderStatusDTO
    {
        public Guid OrderId { get; set; }
        public OrderStatus Status { get; set; }
    }
}
