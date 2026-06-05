using eCommerce.Domain.Entities.Categories;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Domain.Entities.Products
{
    public class Product
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        // Optional base price (used to prefill variants)
        public decimal? BasePrice { get; set; }
        // Does this product use multiple variants?
        public bool HasVariants { get; set; }
        // Publishing control
        public bool IsPublished { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsBestSeller { get; set; } = false;
        public ICollection<ProductVariant> Variants { get; set; } = [];
        public ICollection<ProductImage> Images { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void SoftDelete()
        {
            IsDeleted = true;
        }

        public void SetBestSeller(bool bestSeller)
        {
            IsBestSeller = bestSeller;
        }
    }
}
