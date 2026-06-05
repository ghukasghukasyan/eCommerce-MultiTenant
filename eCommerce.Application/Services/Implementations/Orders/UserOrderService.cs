using eCommerce.Application.DTOs.Addresses;
using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces.Orders;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.QueryFilters;

namespace eCommerce.Application.Services.Implementations.Orders
{
    public class UserOrderService(IUserOrderRepository orderRepository) : IUserOrderService
    {
        public async Task<OrderDetailDTO?> GetByIdAsync(Guid id, string userId)
        {
            var order = await orderRepository.GetByIdAsync(id, userId);

            if (order == null)
                return null;

            var dto = new OrderDetailDTO
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName ?? string.Empty,
                CustomerEmail = order.CustomerEmail,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                ItemsCount = order.Items.Count,
                Status = order.Status,
                ShippingDetail = new ShippingDetailDTO
                {
                    FullName = order.ShippingDetail?.FullName ?? string.Empty,
                    PhoneNumber = order.ShippingDetail?.PhoneNumber ?? string.Empty,
                    City = order.ShippingDetail?.City ?? string.Empty,
                    Address = order.ShippingDetail?.Address ?? string.Empty,
                    Notes = order.ShippingDetail?.Notes
                },

                Items = [.. order.Items.Select(i => new OrderItemDTO
                {
                    VariantId = i.ProductVariantId,
                    ProductId = i.ProductVariant.ProductId,
                    ProductName = i.ProductVariant.Product.Name,
                    ImageUrl = i.ProductVariant.Product.Images
                        .FirstOrDefault(x => x.IsMain)?.ImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    VariantAttributes = i.ProductVariant.AttributeValues
                        .ToDictionary(av => av.VariantAttribute.Name, av => av.Value)
                })]
            };

            return dto;
        }

        public async Task<PagedResult<OrderDTO>> GetAllAsync(OrderFilterDTO filter, string userId)
        {
            OrderQueryFilter domainFilter = new()
            {
                From = filter.From,
                To = filter.To,
                Status = filter.Status
            };

            int skip = (filter.Page - 1) * filter.PageSize;
            int take = filter.PageSize;

            int total = await orderRepository.CountByFilterAsync(domainFilter, userId);

            if (total == 0)
            {
                return new PagedResult<OrderDTO>
                {
                    Items = [],
                    TotalCount = 0
                };
            }

            IEnumerable<Order> orders =
                await orderRepository.GetAsync(domainFilter, userId, skip, take);

            var items = orders.Select(order =>
            {
                return new OrderDTO
                {
                    Id = order.Id,
                    CustomerName = order.CustomerName,
                    CustomerEmail = order.CustomerEmail,
                    CreatedAt = order.CreatedAt,
                    TotalAmount = order.TotalAmount,
                    ItemsCount = order.Items.Count,
                    Status = order.Status
                };
            }).ToList();

            return new PagedResult<OrderDTO>
            {
                Items = items,
                TotalCount = total
            };
        }
    }
}
