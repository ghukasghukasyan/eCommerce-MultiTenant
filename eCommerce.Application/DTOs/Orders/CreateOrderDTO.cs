using eCommerce.Application.DTOs.Addresses;
using eCommerce.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Orders
{
    public class CreateOrderDTO
    {
        [Required]
        public ShippingDetailDTO ShippingDetail { get; set; }
        [Required]
        public List<CreateOrderItemDTO> Items { get; set; } = [];
        [Required]
        public Guid PaymentMethodId { get; set; }
        public string? CouponCode { get; set; }
    }
}
