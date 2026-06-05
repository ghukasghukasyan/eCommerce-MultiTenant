using ClientLibrary.Models.Products;
using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Bases
{
    public class ProductBase
    {
        [Required]
        public string Name { get; init; }
        [Required]
        public string Description { get; init; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; init; }
        [Required]
        public List<ProductImageDTO> Images { get; init; } = [];
        [Required]
        public int Quantity { get; init; }
        [Required]
        public Guid CategoryId { get; init; }
    }
}
