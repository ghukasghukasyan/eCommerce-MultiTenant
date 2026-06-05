using eCommerce.Application.DTOs.Bases;
using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Categories
{
    public class UpdateCategoryDTO : CategoryBaseDTO
    {
        [Required]
        public Guid Id { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
