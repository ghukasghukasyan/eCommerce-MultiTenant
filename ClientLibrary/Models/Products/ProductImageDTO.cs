using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Products
{
    public class ProductImageDTO
    {
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int DisplayOrder { get; set; }
        public string? ObjectPosition { get; set; }
    }

}
