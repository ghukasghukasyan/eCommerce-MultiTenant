using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Products
{
    public class UpdateProductDTO
    {
        [Required]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(200, ErrorMessage = "Product name must not exceed 200 characters.")]
        public string Name { get; set; } = null!;

        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public Guid CategoryId { get; set; }

        // Updates DEFAULT VARIANT
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int Stock { get; set; }
    }
}
