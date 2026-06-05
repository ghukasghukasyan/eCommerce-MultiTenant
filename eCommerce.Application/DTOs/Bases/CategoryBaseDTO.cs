using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Bases
{
    public class CategoryBaseDTO
    {
        [Required]
        public string Name { get; set; }
    }
}
