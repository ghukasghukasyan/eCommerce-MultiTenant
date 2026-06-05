using eCommerce.Domain.Entities.Products;
using System.ComponentModel.DataAnnotations.Schema;

namespace eCommerce.Domain.Entities.Orders
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public Guid ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
