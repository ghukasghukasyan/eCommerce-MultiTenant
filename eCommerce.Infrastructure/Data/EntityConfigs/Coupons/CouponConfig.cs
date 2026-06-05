using eCommerce.Domain.Entities.Coupons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Coupons
{
    public class CouponConfig : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(c => c.Code).IsUnique();

            builder.Property(c => c.DiscountValue).IsRequired().HasPrecision(18, 4);
            builder.Property(c => c.CommissionRate).IsRequired();
        }
    }
}
