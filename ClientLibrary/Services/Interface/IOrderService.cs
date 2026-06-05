using ClientLibrary.Models.Orders;
using ClientLibrary.Models.Responses;

namespace ClientLibrary.Services.Interface
{
    public interface IOrderService
    {
        Task<ServiceResponse<Guid>> SaveOrder(CreateOrderDTO order);
        Task<ServiceResponse<Guid>> Checkout(CheckoutDTO checkout);
        Task<ServiceResponse<Guid>> UpdateStatus(UpdateOrderStatusDTO updateOrderStatus);
        Task<PagedResult<OrderDTO>> GetAll(OrderFilterDTO filter);
        Task<OrderDetailDTO> GetById(Guid id);
        Task<PagedResult<OrderDTO>> GetUserOrders(OrderFilterDTO filter);
        Task<OrderDetailDTO> GeUserOrdertById(Guid id);
    }
}

