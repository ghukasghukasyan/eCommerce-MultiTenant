using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Responses;

namespace eCommerce.Application.Services.Interfaces.Orders
{
    public interface IOrderService
    {
        Task<ServiceResponse> CheckoutAsync(CheckoutDTO checkout);
        Task<ServiceResponse<Guid>> CreateOrderAsync(string userId,CreateOrderDTO order);
        Task<ServiceResponse<Guid>> UpdateStatusAsync(UpdateOrderStatusDTO updateOrderStatus);
        Task<PagedResult<OrderDTO>> GetAllAsync(OrderFilterDTO orderFilter);
        Task<OrderDetailDTO?> GetByIdAsync(Guid id);
    }
}
