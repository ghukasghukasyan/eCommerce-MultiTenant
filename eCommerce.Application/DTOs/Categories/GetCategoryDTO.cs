using eCommerce.Application.DTOs.Bases;

namespace eCommerce.Application.DTOs.Categories
{
    public class GetCategoryDTO : CategoryBaseDTO
    {
        public Guid Id { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string ParentName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
