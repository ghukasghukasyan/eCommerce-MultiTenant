using static ClientLibrary.Enums.Statuses;

namespace ClientLibrary.Models.Orders
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemsCount { get; set; }
        public OrderStatus Status { get; set; }
    }
}
