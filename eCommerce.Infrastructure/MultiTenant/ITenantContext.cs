using System.Text.Json;

namespace eCommerce.Infrastructure.MultiTenant
{
    public interface ITenantContext
    {
        string Slug { get; }
        string ConnectionString { get; }
        JsonElement Config { get; }
    }
}
