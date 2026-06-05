using ClientLibrary.Models.Bases;
using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Categories
{
    public class UpdateCategoryDTO : CategoryBase
    {
        [Required]
        public Guid Id { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
