using ClientLibrary.Models.Addresses;
using static ClientLibrary.Enums.Statuses;

namespace ClientLibrary.Models.Orders
{
    public class OrderDetailDTO
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public List<CartItemDTO> Items { get; set; }
        public int ItemsCount { get; set; }
        public OrderStatus Status { get; set; }
        public ShippingDetailDTO ShippingDetail { get; set; }
        public string CouponCode { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}
