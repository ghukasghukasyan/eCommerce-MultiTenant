using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces.Orders;
using eCommerce.Domain.Entities.Products;
using Stripe.Checkout;

namespace eCommerce.Infrastructure.Services
{
    public class StripePaymentService : IPaymentService
    {
        public async Task<ServiceResponse> Pay(decimal totalAmount, IEnumerable<Product> cartProducts, IEnumerable<OrderItemDTO> carts)
        {
            try
            {
                List<SessionLineItemOptions> lineItems = [];
                foreach (Product item in cartProducts)
                {
                    OrderItemDTO pQuantity = carts.FirstOrDefault(c => c.ProductId == item.Id);
                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "amd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Name,
                                Description = item.Description,
                            },
                            UnitAmountDecimal = (long)((item.BasePrice ?? 0m) * 100),
                        },
                        Quantity = pQuantity!.Quantity,
                    });
                }

                SessionCreateOptions options = new()
                {
                    PaymentMethodTypes = ["card"],
                    LineItems = lineItems,
                    Mode = "payment",
                    SuccessUrl = "https://example.com/success",
                    CancelUrl = "https://example.com/cancel",
                };

                SessionService service = new();
                Session session = await service.CreateAsync(options);
                return new ServiceResponse(true, session.Url!);
            }
            catch (Exception ex)
            {
                return new ServiceResponse(false, ex.Message);
            }
        }
    }
}