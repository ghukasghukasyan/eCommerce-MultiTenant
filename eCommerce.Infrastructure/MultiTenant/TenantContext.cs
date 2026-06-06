using System.Text.Json;

namespace eCommerce.Infrastructure.MultiTenant
{
    public class TenantContext : ITenantContext
    {
        public string Slug { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public JsonElement Config { get; set; }
    }
}
