using System.ComponentModel.DataAnnotations;

namespace eCommerce.Domain.Entities.Products
{
    public class VariantAttributeValue
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductVariantId { get; set; }
        public Guid VariantAttributeId { get; set; }
        public string Value { get; set; } = null!;
        public ProductVariant ProductVariant { get; set; } = null!;
        public VariantAttribute VariantAttribute { get; set; } = null!;
    }
}
