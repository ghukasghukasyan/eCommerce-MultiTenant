using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Coupons
{
    public class UpdateCouponStatusDTO
    {
        public Guid Id { get; set; }
        public ActivityStatus Status { get; set; }
    }
}
