using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Responses;

namespace eCommerce.Application.Services.Interfaces.Orders
{
    public interface IUserOrderService
    {
        Task<PagedResult<OrderDTO>> GetAllAsync(OrderFilterDTO orderFilter, string userId);
        Task<OrderDetailDTO?> GetByIdAsync(Guid id, string userId);
    }
}
