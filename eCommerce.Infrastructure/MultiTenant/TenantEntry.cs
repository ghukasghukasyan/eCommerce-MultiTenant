using System.Text.Json;

namespace eCommerce.Infrastructure.MultiTenant
{
    public class TenantEntry
    {
        public string ConnectionString { get; set; } = string.Empty;
        public JsonElement Config { get; set; }
    }
}
