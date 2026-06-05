using System.ComponentModel.DataAnnotations;

namespace eCommerce.Domain.Entities.Products
{
    public class ProductImage
    {
        [Key]
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
        public string? ObjectPosition { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
