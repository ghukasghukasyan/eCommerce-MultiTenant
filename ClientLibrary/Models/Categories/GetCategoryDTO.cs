using ClientLibrary.Models.Bases;

namespace ClientLibrary.Models.Categories
{
    public class GetCategoryDTO : CategoryBase
    {
        public Guid Id { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public string ParentName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
