using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Bases
{
    public class CategoryBase
    {
        [Required]
        public string Name { get; set; }   
    }
}
