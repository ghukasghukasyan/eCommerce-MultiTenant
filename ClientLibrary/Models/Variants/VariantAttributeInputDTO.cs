namespace ClientLibrary.Models.Variants
{
    public class VariantAttributeInputDTO
    {
        public Guid AttributeId { get; set; }
        public List<string> Values { get; set; } = new();
    }
}
