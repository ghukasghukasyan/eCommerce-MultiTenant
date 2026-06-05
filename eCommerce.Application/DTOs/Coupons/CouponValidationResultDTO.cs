namespace eCommerce.Application.DTOs.Coupons
{
    public class CouponValidationResultDTO
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        public Guid CouponId { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountedTotal { get; set; }
    }
}
