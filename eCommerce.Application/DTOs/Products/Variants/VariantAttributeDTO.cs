namespace eCommerce.Application.DTOs.Products.Variants
{
    public class VariantAttributeDTO
    {
        public Guid AttributeId { get; set; }
        public List<string> Values { get; set; } = [];
    }
}
