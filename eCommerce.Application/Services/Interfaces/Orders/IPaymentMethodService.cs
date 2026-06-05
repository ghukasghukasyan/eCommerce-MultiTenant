using eCommerce.Application.DTOs.Orders;

namespace eCommerce.Application.Services.Interfaces.Orders
{
    public interface IPaymentMethodService
    {
        Task<IEnumerable<GetPaymentMethodDTO>> GetMethodsAsync();
    }
}
