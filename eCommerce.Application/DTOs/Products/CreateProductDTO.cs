using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Products
{
    public class CreateProductDTO
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; }

        [Required]
        public Guid CategoryId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal InitialPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int InitialStock { get; set; }

        public bool HasVariants { get; set; } = false;
    }
}
