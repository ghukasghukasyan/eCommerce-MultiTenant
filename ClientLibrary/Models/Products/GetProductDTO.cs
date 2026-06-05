using ClientLibrary.Models.Categories;
using ClientLibrary.Models.Variants;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClientLibrary.Models.Products
{
    public class GetProductDTO
    {
        [Required]
        public Guid Id { get; set; }
        public Guid DefaultVariantId { get; set; }
        public string Name { get; init; }
        public string Description { get; init; }
        public Guid CategoryId { get; init; }
        public GetCategoryDTO Category { get; set; }
        [DataType(DataType.Currency)]
        public decimal Price { get; init; }
        [Required]
        public List<ProductImageDTO> Images { get; init; } = [];
        [Required]
        public int Stock { get; set; }
        [Required]
        [JsonPropertyName("createdAt")]
        public DateTime CreatedDate { get; set; }
        public bool IsNew => DateTime.UtcNow <= CreatedDate.AddDays(7);
        public bool IsBestSeller { get; set; }
        public List<VariantDTO> Variants { get; set; } = new();
    }
}
