using eCommerce.Application.DTOs.Addresses;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Orders
{
    public class OrderDetailDTO
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemDTO> Items { get; set; }
        public int ItemsCount { get; set; }
        public OrderStatus Status { get; set; }
        public ShippingDetailDTO ShippingDetail { get; set; }
        public string CouponCode { get; set; }
        public decimal? DiscountAmount { get; set; }
    }
}
