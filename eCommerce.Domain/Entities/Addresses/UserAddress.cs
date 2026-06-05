namespace eCommerce.Domain.Entities.Addresses
{
    public class UserAddress
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string City { get; set; }
        public string AddressLine { get; set; }
        public string PostalCode { get; set; }
        public string Notes { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
