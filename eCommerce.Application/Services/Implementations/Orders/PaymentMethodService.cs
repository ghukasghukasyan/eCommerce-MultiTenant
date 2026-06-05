using AutoMapper;
using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.Services.Interfaces.Orders;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Orders;
using Microsoft.Extensions.Caching.Memory;

namespace eCommerce.Application.Services.Implementations.Orders
{
    internal class PaymentMethodService(IPaymentMethodRepository paymentMethod, IMapper mapper, IMemoryCache cache) : IPaymentMethodService
    {
        private const string CacheKey = "payment-methods";

        public async Task<IEnumerable<GetPaymentMethodDTO>> GetMethodsAsync()
        {
            if (cache.TryGetValue(CacheKey, out IEnumerable<GetPaymentMethodDTO>? cached))
                return cached!;

            IEnumerable<PaymentMethod> paymentMethods = await paymentMethod.GetPaymentMethodsAsync();
            if (!paymentMethods.Any()) return [];

            var mapped = mapper.Map<IEnumerable<GetPaymentMethodDTO>>(paymentMethods);
            cache.Set(CacheKey, mapped, TimeSpan.FromHours(1));
            return mapped;
        }
    }
}
