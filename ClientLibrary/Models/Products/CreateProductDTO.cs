using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Products
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(200, ErrorMessage = "Product name must not exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; }

        [Required(ErrorMessage = "Category is required.")]
        public Guid CategoryId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal InitialPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int InitialStock { get; set; }

        public bool HasVariants { get; set; } = false;
    }
}
