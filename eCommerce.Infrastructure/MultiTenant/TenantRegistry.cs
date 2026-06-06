using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace eCommerce.Infrastructure.MultiTenant
{
    public class TenantRegistry
    {
        private readonly Dictionary<string, TenantEntry> _tenants;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public TenantRegistry(IConfiguration config, IHostEnvironment env)
        {
            var configPath = config["Tenants:ConfigPath"] ?? "/app/tenants.json";

            // Resolve relative paths from the content root so dotnet run works from any directory
            var path = Path.IsPathRooted(configPath)
                ? configPath
                : Path.Combine(env.ContentRootPath, configPath);

            using var stream = File.OpenRead(path);
            _tenants = JsonSerializer.Deserialize<Dictionary<string, TenantEntry>>(stream, _jsonOptions) ?? new();
        }

        public bool TryGet(string slug, out TenantEntry entry) =>
            _tenants.TryGetValue(slug, out entry!);

        public IEnumerable<KeyValuePair<string, TenantEntry>> All => _tenants;
    }
}
