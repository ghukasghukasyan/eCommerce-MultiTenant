using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Addresses
{
    public class ShippingDetailDTO
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Address { get; set; }
        public string PostalCode { get; set; }
        public string Notes { get; set; }
    }
}
