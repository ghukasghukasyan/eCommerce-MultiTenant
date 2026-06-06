using System.Net.Http.Json;
using ClientLibrary.Constants;
using ClientLibrary.Helpers.Implementation;
using ClientLibrary.Helpers.Interface;
using ClientLibrary.Services.Implementation;
using ClientLibrary.Services.Interface;
using ClientLibrary.States;
using eCommerce.Frontend;
using eCommerce.Frontend.Authentication;
using eCommerce.Frontend.Models;
using eCommerce.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Caching.Memory;
using eCommerce.Frontend.Localization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Load tenant config from the API before registering services
var bootstrapHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
TenantConfig tenant;
try { tenant = await bootstrapHttp.GetFromJsonAsync<TenantConfig>("api/tenant") ?? new TenantConfig(); }
catch { tenant = new TenantConfig(); }
builder.Services.AddSingleton(tenant);

builder.Services.AddMemoryCache();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<ICookieService, CookieService>();

// SERVICES
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<UiLocalizer>();
builder.Services.AddScoped<IHttpClientHelper, HttpClientHelper>();
builder.Services.AddScoped<IApiCallHelper, ApiCallHelper>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInfluencerService, InfluencerService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<IVariantService, VariantService>();
builder.Services.AddScoped<IVariantAttributeService, VariantAttributeService>();
builder.Services.AddTransient<RefreshTokenHandler>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<CartHelper>();
builder.Services.AddScoped<CartState>();
builder.Services.AddSingleton<OrderNotificationState>();
builder.Services.AddSingleton<UiOverlayState>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
// In multi-tenant, the API is always on the same origin proxied at /api/
var apiBaseUrl = "/api/";

Console.WriteLine($"ENV: {builder.HostEnvironment.Environment}");
Console.WriteLine($"Tenant: {tenant.StoreName}");

// NAMED CLIENT
builder.Services.AddHttpClient(AppConstants.ApiClient.Name, client =>
{
    var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.BaseAddress = new Uri(baseAddress, apiBaseUrl); // ✅ THE FIX
})
.AddHttpMessageHandler<RefreshTokenHandler>();
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient(AppConstants.ApiClient.Name);
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();