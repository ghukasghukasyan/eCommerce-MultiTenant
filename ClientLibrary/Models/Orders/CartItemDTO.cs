using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Orders
{
    public class CartItemDTO
    {
        [Required]
        public Guid VariantId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public Dictionary<string, string>? VariantAttributes { get; set; }

    }
}
