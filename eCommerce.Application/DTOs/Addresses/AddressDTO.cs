namespace eCommerce.Application.DTOs.Addresses
{
    public class AddressDTO
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string AddressLine { get; set; }
        public string PostalCode { get; set; }
        public string Notes { get; set; }
        public bool IsDefault { get; set; }
    }
}
