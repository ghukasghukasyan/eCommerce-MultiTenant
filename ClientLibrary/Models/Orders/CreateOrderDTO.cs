using ClientLibrary.Models.Addresses;
using ClientLibrary.Enums;
using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Orders
{
    public class  CreateOrderDTO
    {
        [Required]
        public List<CreateOrderItemDTO> Items { get; set; } = [];
        [Required]
        public ShippingDetailDTO ShippingDetail { get; set; }
        [Required]
        public Guid PaymentMethodId { get; set; }
        public string? CouponCode { get; set; }
    }
}
