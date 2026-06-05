namespace ClientLibrary.Models.Variants
{
    public class VariantDTO
    {
        public Guid VariantId { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new();
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
