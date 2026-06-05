using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Orders
{
    public class CheckoutDTO
    {
        [Required]
        required public Guid PaymentMethodId { get; set; }
        [Required]
        public Guid OrderId { get; set; }
    }
}
