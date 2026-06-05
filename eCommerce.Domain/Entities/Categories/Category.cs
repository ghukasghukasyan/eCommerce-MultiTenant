using eCommerce.Domain.Entities.Products;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Domain.Entities.Categories
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public Guid? ParentCategoryId { get; set; }
        public Category ParentCategory { get; set; }
        public ICollection<Category> Children { get; set; } = [];
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public string ImageUrl { get; set; }
        public string Icon { get; set; }
        public ICollection<Product> Products { get; set; } = [];
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
