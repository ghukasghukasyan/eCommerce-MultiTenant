using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Orders
{
    public class OrderItemDTO
    {
        [Required]
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public Dictionary<string, string> VariantAttributes { get; set; } = new();
    }
}
