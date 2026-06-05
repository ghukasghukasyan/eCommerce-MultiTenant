using eCommerce.Application.DTOs.Addresses;
using eCommerce.Application.DTOs.Coupons;
using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Implementations.Coupons;
using eCommerce.Application.Services.Interfaces;
using eCommerce.Application.Services.Interfaces.Notifications;
using eCommerce.Application.Services.Interfaces.Orders;
using eCommerce.Domain.Entities.Addresses;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Coupons;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.Interfaces.Products;
using eCommerce.Domain.QueryFilters;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.Services.Implementations.Orders
{
    public class OrderService(
     IOrderRepository orderRepository,
     IVariantRepository variantRepository,
     IPaymentMethodService paymentMethodService,
     //IPaymentService paymentService,
     IUserManagement userManagement,
     ICouponRepository couponRepository,
     IOrderNotificationService notificationService,
     IUnitOfWork unitOfWork
 ) : IOrderService
    {

        private sealed class OrderFailureException(ServiceResponse<Guid> result) : Exception
        {
            public ServiceResponse<Guid> Result { get; } = result;
        }

        public async Task<ServiceResponse<Guid>> CreateOrderAsync(string userId, CreateOrderDTO request)
        {
            if (request.Items.Count == 0)
                return new ServiceResponse<Guid>(false, Guid.Empty, "Order must contain at least one item.");

            var user = await userManagement.GetByIdAsync(userId);
            if (user == null)
                return new ServiceResponse<Guid>(false, Guid.Empty, "User not found.");

            if (request.PaymentMethodId == Guid.Empty)
                return new ServiceResponse<Guid>(false, Guid.Empty, "Payment method is required.");

            var paymentMethods = await paymentMethodService.GetMethodsAsync();
            if (!paymentMethods.Any(x => x.Id == request.PaymentMethodId))
                return new ServiceResponse<Guid>(false, Guid.Empty, "Invalid payment method.");

            Order order = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CustomerName = user.FullName,
                CustomerEmail = user.Email,
                PaymentMethodId = request.PaymentMethodId,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ShippingDetail = new ShippingDetail
                {
                    FullName = request.ShippingDetail.FullName,
                    PhoneNumber = request.ShippingDetail.PhoneNumber,
                    City = request.ShippingDetail.City,
                    Address = request.ShippingDetail.Address,
                    PostalCode = request.ShippingDetail.PostalCode,
                    Notes = request.ShippingDetail.Notes
                }
            };

            try
            {
                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    decimal totalAmount = 0;

                    foreach (CreateOrderItemDTO item in request.Items)
                    {
                        var variant = await variantRepository.GetByIdAsync(item.VariantId);

                        if (variant == null)
                            throw new OrderFailureException(new ServiceResponse<Guid>(false, Guid.Empty, "Variant not found."));

                        if (!variant.IsActive)
                            throw new OrderFailureException(new ServiceResponse<Guid>(false, Guid.Empty, "Variant is inactive."));

                        // Atomic stock decrement: succeeds only if StockQuantity >= Quantity in the DB right now
                        bool decremented = await variantRepository.DecrementStockAsync(variant.Id, item.Quantity);
                        if (!decremented)
                            throw new OrderFailureException(new ServiceResponse<Guid>(false, Guid.Empty, "Insufficient stock."));

                        totalAmount += variant.Price * item.Quantity;

                        order.Items.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductVariantId = variant.Id,
                            Quantity = item.Quantity,
                            UnitPrice = variant.Price
                        });
                    }

                    order.TotalAmount = totalAmount;

                    // Apply coupon if provided
                    if (!string.IsNullOrWhiteSpace(request.CouponCode))
                    {
                        var coupon = await couponRepository.GetByCodeAsync(request.CouponCode);
                        if (coupon != null && coupon.Status == ActivityStatus.Active
                            && (!coupon.ExpiryDate.HasValue || coupon.ExpiryDate.Value >= DateTime.UtcNow)
                            && (!coupon.MaxUsages.HasValue || coupon.UsedCount < coupon.MaxUsages.Value))
                        {
                            var discountAmount = CouponService.CalculateDiscount(coupon, totalAmount);
                            order.TotalAmount = Math.Max(0, totalAmount - discountAmount);

                            coupon.UsedCount++;
                            await couponRepository.UpdateAsync(coupon);

                            await couponRepository.AddCouponOrderAsync(new CouponOrder
                            {
                                Id = Guid.NewGuid(),
                                CouponId = coupon.Id,
                                OrderId = order.Id,
                                DiscountAmount = discountAmount,
                                CommissionAmount = Math.Round(discountAmount * coupon.CommissionRate / 100m, 2),
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    await orderRepository.SaveAsync(order);
                });
            }
            catch (OrderFailureException ex)
            {
                return ex.Result;
            }

            await notificationService.NotifyNewOrderAsync(new NewOrderEvent(
                order.Id,
                order.CustomerName ?? string.Empty,
                order.CustomerEmail ?? string.Empty,
                order.ShippingDetail?.PhoneNumber ?? string.Empty,
                order.TotalAmount,
                order.CreatedAt));

            return new ServiceResponse<Guid>(true, order.Id, "Order created.");
        }

        public async Task<ServiceResponse> CheckoutAsync(CheckoutDTO checkout)
        {
            var order = await orderRepository.GetByIdAsync(checkout.OrderId);

            if (order == null)
                return new ServiceResponse(false, "Order not found.");

            if (order.Status != OrderStatus.Pending)
                return new ServiceResponse(false, "Invalid order status.");

            var paymentMethods = await paymentMethodService.GetMethodsAsync();

            if (!paymentMethods.Any(p => p.Id == checkout.PaymentMethodId))
                return new ServiceResponse(false, "Invalid payment method.");

            // payment gateway integration placeholder

            var updated = await orderRepository.UpdateCheckoutAsync(checkout.OrderId);
            if (updated == 0)
                return new ServiceResponse(false, "Failed to update order status.");

            return new ServiceResponse(true, "Payment successful.");
        }

        public async Task<ServiceResponse<Guid>> UpdateStatusAsync(UpdateOrderStatusDTO updateOrderStatus)
        {
            var order = await orderRepository.GetByIdAsync(updateOrderStatus.OrderId);

            if (order == null)
                return new ServiceResponse<Guid>(false, Guid.Empty, "Order not found");

            if (order.Status == updateOrderStatus.Status)
                return new ServiceResponse<Guid>(true, order.Id, "Status unchanged");

            if (!IsValidStatusTransition(order.Status, updateOrderStatus.Status))
                return new ServiceResponse<Guid>(
                    false,
                    Guid.Empty,
                    $"Cannot change order status from {order.Status} to {updateOrderStatus.Status}"
                );

            var result = await orderRepository.UpdateStatus(updateOrderStatus.OrderId, updateOrderStatus.Status);

            if (result == 1 && updateOrderStatus.Status == OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                    await variantRepository.IncrementStockAsync(item.ProductVariantId, item.Quantity);
            }

            return result == 1
                ? new ServiceResponse<Guid>(true, updateOrderStatus.OrderId, "Updated successfully")
                : new ServiceResponse<Guid>(false, Guid.Empty, "Order status not updated");
        }

        public async Task<OrderDetailDTO?> GetByIdAsync(Guid id)
        {
            var order = await orderRepository.GetByIdAsync(id);

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

            var couponOrder = await couponRepository.GetCouponOrderByOrderIdAsync(order.Id);
            if (couponOrder != null)
            {
                var coupon = await couponRepository.GetByIdAsync(couponOrder.CouponId);
                dto.CouponCode = coupon?.Code;
                dto.DiscountAmount = couponOrder.DiscountAmount;
            }

            return dto;
        }

        public async Task<PagedResult<OrderDTO>> GetAllAsync(OrderFilterDTO filter)
        {
            OrderQueryFilter domainFilter = new()
            {
                From = filter.From,
                To = filter.To,
                Status = filter.Status
            };

            int skip = (filter.Page - 1) * filter.PageSize;
            int take = filter.PageSize;

            int total = await orderRepository.CountByFilterAsync(domainFilter);

            if (total == 0)
            {
                return new PagedResult<OrderDTO>
                {
                    Items = [],
                    TotalCount = 0
                };
            }

            IEnumerable<Order> orders =
                await orderRepository.GetAsync(domainFilter, skip, take);

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

        private static bool IsValidStatusTransition(OrderStatus current, OrderStatus next)
        {
            return current switch
            {
                OrderStatus.Pending =>
                    next is OrderStatus.Paid or OrderStatus.Cancelled or OrderStatus.Failed,

                OrderStatus.Paid =>
                    next is OrderStatus.Shipped or OrderStatus.Cancelled,

                OrderStatus.Shipped =>
                    next is OrderStatus.Completed,

                OrderStatus.Completed => false,
                OrderStatus.Cancelled => false,
                OrderStatus.Failed => false,

                _ => false
            };
        }
    }
}
