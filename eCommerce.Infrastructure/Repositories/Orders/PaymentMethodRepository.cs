using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Orders
{
    public class PaymentMethodRepository(ECommerceContext context) : IPaymentMethodRepository
    {
        public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync() => await context.PaymentMethods.AsNoTracking().ToListAsync();
    }
}
