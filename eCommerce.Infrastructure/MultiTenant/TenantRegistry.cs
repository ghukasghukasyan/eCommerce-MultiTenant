using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace eCommerce.Infrastructure.MultiTenant
{
    public class TenantRegistry
    {
        private readonly Dictionary<string, TenantEntry> _tenants;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public TenantRegistry(IConfiguration config)
        {
            var path = config["Tenants:ConfigPath"] ?? "/app/tenants.json";
            using var stream = File.OpenRead(path);
            _tenants = JsonSerializer.Deserialize<Dictionary<string, TenantEntry>>(stream, _jsonOptions) ?? new();
        }

        public bool TryGet(string slug, out TenantEntry entry) =>
            _tenants.TryGetValue(slug, out entry!);

        public IEnumerable<KeyValuePair<string, TenantEntry>> All => _tenants;
    }
}
