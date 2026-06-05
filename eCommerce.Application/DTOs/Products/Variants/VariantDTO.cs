namespace eCommerce.Application.DTOs.Products.Variants
{
    public class VariantDTO
    {
        public Guid VariantId { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = [];
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
