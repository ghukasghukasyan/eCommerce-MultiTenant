namespace eCommerce.Domain.Entities.Addresses
{
    public class ShippingDetail
    {
        public string FullName { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string City { get; set; } = "";
        public string Address { get; set; } = "";
        public string PostalCode { get; set; }
        public string Notes { get; set; }
    }
}
