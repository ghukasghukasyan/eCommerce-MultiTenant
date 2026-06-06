using eCommerce.Application.Services.Interfaces;
using eCommerce.Application.Services.Interfaces.Files;
using eCommerce.Application.Services.Interfaces.Logging;
using eCommerce.Application.Services.Interfaces.Orders;
using eCommerce.Domain.Entities.Categories;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Interfaces;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Categories;
using eCommerce.Domain.Interfaces.Coupons;
using eCommerce.Domain.Interfaces.Influencers;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.Interfaces.Products;
using eCommerce.Domain.Interfaces.Users;
using eCommerce.Infrastructure.Data;
using eCommerce.Infrastructure.Middlewares;
using eCommerce.Infrastructure.MultiTenant;
using eCommerce.Infrastructure.Repositories;
using eCommerce.Infrastructure.Repositories.Authentication;
using eCommerce.Infrastructure.Repositories.Categories;
using eCommerce.Infrastructure.Repositories.Coupons;
using eCommerce.Infrastructure.Repositories.Influencers;
using eCommerce.Infrastructure.Repositories.Orders;
using eCommerce.Infrastructure.Repositories.Products;
using eCommerce.Infrastructure.Repositories.Users;
using eCommerce.Infrastructure.Services;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace eCommerce.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<TenantRegistry>();
            services.AddScoped<TenantContext>();
            services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

            services.AddDbContext<ECommerceContext>((serviceProvider, options) =>
            {
                var tenant = serviceProvider.GetRequiredService<ITenantContext>();
                options.UseNpgsql(tenant.ConnectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(ServiceContainer).Assembly.FullName);
                    sql.EnableRetryOnFailure();
                }).UseExceptionProcessor();
            }, ServiceLifetime.Scoped);

            services.AddScoped(typeof(IAppLogger<>), typeof(SerilogLoggerAdapter<>));
            services.AddDefaultIdentity<AppUser>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredUniqueChars = 1;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ECommerceContext>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwtKey = config["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(jwtKey))
                    throw new InvalidOperationException("JWT key is not configured. Set the 'Jwt:Key' config value or the JWT_KEY environment variable.");

                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
                };

                // Allow JWT via query string for SignalR WebSocket connections
                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken) &&
                            context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddHttpClient("Brevo", c =>
            {
                c.DefaultRequestHeaders.Add("api-key", config["Email:ApiKey"] ?? "");
                c.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                c.Timeout = TimeSpan.FromSeconds(15);
            });
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenManagement, TokenManagement>();
            services.AddScoped<IRoleManagement, RoleManagement>();
            services.AddScoped<IUserManagement, UserManagement>();
            services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IGeneric<Category>, GenericRepository<Category>>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IInfluencerRepository, InfluencerRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IVariantRepository, ProductVariantRepository>();
            services.AddScoped<IVariantAttributeRepository, VariantAttributeRepository>();
            services.AddScoped<IUserOrderRepository, UserOrderRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public static IApplicationBuilder UseInfrastructureService(this IApplicationBuilder app)
        {
            app.UseMiddleware<TenantMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }
    }
}

