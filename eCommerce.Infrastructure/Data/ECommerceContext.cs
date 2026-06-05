using eCommerce.Domain.Entities.Addresses;
using eCommerce.Domain.Entities.Categories;
using eCommerce.Domain.Entities.Coupons;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Entities.Influencers;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Entities.Products;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Infrastructure.Data
{
    public class ECommerceContext(DbContextOptions<ECommerceContext> options) : IdentityDbContext<AppUser>(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponOrder> CouponOrders { get; set; }
        public DbSet<Influencer> Influencers { get; set; }
        public DbSet<InfluencerPayout> InfluencerPayouts { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<VariantAttribute> VariantAttributes { get; set; }
        public DbSet<VariantAttributeValue> VariantAttributeValues { get; set; }

        public DbSet<UserAddress> UserAddresses { get; set; }

        //TODO poxel roleri inserti logicy
        protected override void OnModelCreating(ModelBuilder builder) 
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(ECommerceContext).Assembly);
            
            builder.Entity<IdentityRole>().HasData(
        new IdentityRole
        {
            Id = "380fd12a-a2c7-402d-a806-9c964adc6e0e",
            Name = "Admin",
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "1"
        },
        new IdentityRole
        {
            Id = "eccb8de8-6d52-4bb8-b9d9-889c0e49071b",
            Name = "User",
            NormalizedName = "USER",
            ConcurrencyStamp = "2"
        }, new IdentityRole
        {
            Id = "bagb8de8-6d52-4bb8-b9d9-889c0e49071b",
            Name = "Influencer",
            NormalizedName = "INFLUENCER",
            ConcurrencyStamp = "2"
        }

    );
        }
    }
}
