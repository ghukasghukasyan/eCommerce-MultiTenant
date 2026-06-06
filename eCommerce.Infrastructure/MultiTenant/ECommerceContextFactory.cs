using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace eCommerce.Infrastructure.MultiTenant
{
    // Used only by `dotnet ef migrations` at design time — not at runtime.
    public class ECommerceContextFactory : IDesignTimeDbContextFactory<ECommerceContext>
    {
        public ECommerceContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ECommerceContext>()
                .UseNpgsql(
                    "Host=localhost;Port=5432;Database=ecommerce_design;Username=postgres;Password=6776",
                    sql => sql.MigrationsAssembly(typeof(ECommerceContextFactory).Assembly.FullName))
                .Options;

            return new ECommerceContext(options);
        }
    }
}
