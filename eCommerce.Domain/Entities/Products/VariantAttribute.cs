using System.ComponentModel.DataAnnotations;

namespace eCommerce.Domain.Entities.Products
{
    public class VariantAttribute
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!; // Size, Color, Volume
    }
}
