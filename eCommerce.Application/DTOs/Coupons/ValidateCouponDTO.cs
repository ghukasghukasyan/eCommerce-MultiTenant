namespace eCommerce.Application.DTOs.Coupons
{
    public class ValidateCouponDTO
    {
        public string Code { get; set; }
        public decimal OrderTotal { get; set; }
    }
}
