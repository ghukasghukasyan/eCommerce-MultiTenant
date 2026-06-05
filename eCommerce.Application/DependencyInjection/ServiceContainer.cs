using eCommerce.Application.Mappings;
using eCommerce.Application.Services.Implementations.Authentication;
using eCommerce.Application.Services.Implementations.Categories;
using eCommerce.Application.Services.Implementations.Coupons;
using eCommerce.Application.Services.Implementations.Influencers;
using eCommerce.Application.Services.Implementations.Orders;
using eCommerce.Application.Services.Implementations.Products;
using eCommerce.Application.Services.Implementations.Users;
using eCommerce.Application.Services.Interfaces.Authentication;
using eCommerce.Application.Services.Interfaces.Categories;
using eCommerce.Application.Services.Interfaces.Coupons;
using eCommerce.Application.Services.Interfaces.Influencers;
using eCommerce.Application.Services.Interfaces.Orders;
using eCommerce.Application.Services.Interfaces.Products;
using eCommerce.Application.Services.Interfaces.Users;
using eCommerce.Application.Services.Interfaces;
using eCommerce.Application.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Application.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddApplicationService(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddAutoMapper(typeof(MappingConfig).Assembly);
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPaymentMethodService, PaymentMethodService>();
            services.AddScoped<IInfluencerService, InfluencerService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IVariantAttributeService, VariantAttributeService>();
            services.AddScoped<IVariantService, VariantService>();
            services.AddScoped<IUserOrderService, UserOrderService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
