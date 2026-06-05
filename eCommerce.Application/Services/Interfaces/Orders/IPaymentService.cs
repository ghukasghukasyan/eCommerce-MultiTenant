using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Domain.Entities.Products;

namespace eCommerce.Application.Services.Interfaces.Orders
{
    public interface IPaymentService
    {
        Task<ServiceResponse> Pay(decimal totalAmount, IEnumerable<Product> cartProducts, IEnumerable<OrderItemDTO> carts);
    }
}
