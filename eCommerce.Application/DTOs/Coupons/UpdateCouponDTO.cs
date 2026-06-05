using System.ComponentModel.DataAnnotations;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Coupons
{
    public class UpdateCouponDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Code { get; set; }

        public DiscountType DiscountType { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Discount value must be greater than 0")]
        public decimal DiscountValue { get; set; }

        [Range(0, 100, ErrorMessage = "Commission rate must be between 0 and 100")]
        public int CommissionRate { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max usages must be at least 1")]
        public int? MaxUsages { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}
