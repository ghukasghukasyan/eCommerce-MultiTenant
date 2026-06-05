namespace eCommerce.Application.DTOs.Products.Variants
{
    public class GenerateVariantsDTO
    {
        public Guid ProductId { get; set; }
        public List<VariantAttributeInputDTO> Attributes { get; set; } = [];
    }
}
