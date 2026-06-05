using ClientLibrary.Models.Bases;

namespace ClientLibrary.Models.Categories
{
    public class CreateCategoryDTO : CategoryBase
    {
        public Guid? ParentCategoryId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
