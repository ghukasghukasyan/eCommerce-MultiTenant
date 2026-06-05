namespace eCommerce.Domain.Entities.Orders
{
    public class CouponOrder
    {
        public Guid Id { get; set; }
        public Guid CouponId { get; set; }
        public Guid OrderId { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
