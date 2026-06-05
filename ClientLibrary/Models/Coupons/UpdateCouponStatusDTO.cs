using static ClientLibrary.Enums.Statuses;

namespace ClientLibrary.Models.Coupons
{
    public class UpdateCouponStatusDTO
    {
        public Guid Id { get; set; }
        public ActivityStatus Status { get; set; }
    }
}
