using System.ComponentModel.DataAnnotations;

namespace ClientLibrary.Models.Addresses
{
    public class ShippingDetailDTO
    {
        [Required(ErrorMessage = "Recipient full name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        public string PostalCode { get; set; }
        public string Notes { get; set; }
    }
}
