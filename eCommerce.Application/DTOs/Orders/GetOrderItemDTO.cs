namespace eCommerce.Application.DTOs.Orders
{
    public class GetOrderItemDTO
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
