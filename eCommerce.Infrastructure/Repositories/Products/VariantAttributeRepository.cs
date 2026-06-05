using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Repositories.Products
{
    public class VariantAttributeRepository(ECommerceContext context)
     : IVariantAttributeRepository
    {
        public async Task AddAsync(VariantAttribute attribute)
            => await context.VariantAttributes.AddAsync(attribute);

        public async Task<List<VariantAttribute>> GetAllAsync()
            => await context.VariantAttributes.AsNoTracking().ToListAsync();

        public async Task SaveAsync()
          => await context.SaveChangesAsync();
    }
}
