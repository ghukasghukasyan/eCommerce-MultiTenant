namespace eCommerce.Application.DTOs.Products.Variants
{
    public class VariantAttributeInputDTO
    {
        public Guid AttributeId { get; set; }
        public List<string> Values { get; set; } = [];
    }
}
