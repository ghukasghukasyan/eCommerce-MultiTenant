using System.ComponentModel.DataAnnotations;namespace eCommerce.Application.DTOs.Orders
{
    public class CreateOrderItemDTO
    {
        [Required]
        public Guid VariantId { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
