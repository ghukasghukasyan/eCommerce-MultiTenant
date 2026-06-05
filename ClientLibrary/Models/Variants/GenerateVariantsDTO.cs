namespace ClientLibrary.Models.Variants
{
    public class GenerateVariantsDTO
    {
        public Guid ProductId { get; set; }
        public List<VariantAttributeInputDTO> Attributes { get; set; } = new();
    }
}
