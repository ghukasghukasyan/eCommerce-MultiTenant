using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Products.Variants
{
    public class UpdateVariantDTO
    {
        [Required]
        public Guid VariantId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        public bool IsActive { get; set; }
    }
}
