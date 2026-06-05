using eCommerce.Application.DTOs.Products.Variants;

namespace eCommerce.Application.DTOs.Products
{
    public class GetProductDTO
    {
        public Guid Id { get; set; }
        public Guid DefaultVariantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public bool IsBestSeller { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public List<ProductImageDTO> Images { get; set; } = [];
        public List<VariantDTO> Variants { get; set; } = [];
    }
}
