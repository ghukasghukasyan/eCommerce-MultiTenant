namespace eCommerce.Application.Services.Interfaces.Notifications
{
    public record NewOrderEvent(
        Guid OrderId,
        string CustomerName,
        string CustomerEmail,
        string CustomerPhone,
        decimal TotalAmount,
        DateTime CreatedAt);

    public interface IOrderNotificationService
    {
        Task NotifyNewOrderAsync(NewOrderEvent order);
    }
}
