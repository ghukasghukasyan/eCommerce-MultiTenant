namespace eCommerce.Application.DTOs.Orders
{
    public class GetPaymentMethodDTO
    {
        required public Guid Id { get; set; }
        required public string Name { get; set; }

    }
}
