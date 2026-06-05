using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Orders
{
    public class CreateOrderItemDTO
    {
        [Required]
        public Guid VariantId { get; set; }
        public Guid ProductId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
