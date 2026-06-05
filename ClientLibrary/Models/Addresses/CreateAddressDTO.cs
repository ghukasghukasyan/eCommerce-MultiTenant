namespace ClientLibrary.Models.Addresses
{
    public class CreateAddressDTO
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string AddressLine { get; set; }
        public string Notes { get; set; }
        public string PostalCode { get; set; }
        public bool IsDefault { get; set; }
    }
}
