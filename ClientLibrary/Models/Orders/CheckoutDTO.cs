using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Orders
{
    public class CheckoutDTO
    {
        [Required]
        public Guid PaymentMethodId { get; set; }
        [Required]
        public IEnumerable<CartItemDTO> OrderItems { get; set; } = [];
    }
}
