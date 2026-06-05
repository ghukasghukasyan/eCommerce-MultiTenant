using eCommerce.Application.DTOs.Bases;

namespace eCommerce.Application.DTOs.Categories
{
    public class CreateCategoryDTO : CategoryBaseDTO
    {
        public Guid? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
