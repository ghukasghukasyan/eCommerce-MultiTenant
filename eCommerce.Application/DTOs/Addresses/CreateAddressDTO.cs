using System.ComponentModel.DataAnnotations;

namespace eCommerce.Application.DTOs.Addresses
{
    public class CreateAddressDTO
    {
        [Required]
        [StringLength(150)]
        public string FullName { get; set; }

        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(300)]
        public string AddressLine { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        [StringLength(20)]
        public string PostalCode { get; set; }

        public bool IsDefault { get; set; }
    }
}
