using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace eCommerce.Infrastructure.MultiTenant
{
    public class TenantMiddleware(RequestDelegate next, IConfiguration config)
    {
        private static readonly HashSet<string> _bypassPaths =
            new(StringComparer.OrdinalIgnoreCase) { "/health", "/swagger", "/swagger/index.html" };

        public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, TenantRegistry registry)
        {
            if (_bypassPaths.Contains(context.Request.Path))
            {
                await next(context);
                return;
            }

            var host = context.Request.Host.Host;
            var slug = host.Split('.')[0];

            if (!registry.TryGet(slug, out var entry))
            {
                // In development fall back to the configured default tenant so localhost works
                var devDefault = config["Tenants:DevDefault"];
                if (devDefault is not null && registry.TryGet(devDefault, out entry))
                    slug = devDefault;
                else
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync($"Tenant '{slug}' not found.");
                    return;
                }
            }

            tenantContext.Slug = slug;
            tenantContext.ConnectionString = entry.ConnectionString;
            tenantContext.Config = entry.Config;

            await next(context);
        }
    }
}
