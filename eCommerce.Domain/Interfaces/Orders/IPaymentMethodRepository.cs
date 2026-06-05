using eCommerce.Domain.Entities.Orders;

namespace eCommerce.Domain.Interfaces.Orders
{
    public interface IPaymentMethodRepository
    {
        Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync();
    }
}
