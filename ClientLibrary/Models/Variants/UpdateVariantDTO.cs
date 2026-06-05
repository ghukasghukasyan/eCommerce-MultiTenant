namespace ClientLibrary.Models.Variants
{
    public class UpdateVariantDTO
    {
        public Guid VariantId { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
