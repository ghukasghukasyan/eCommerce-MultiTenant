using eCommerce.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Orders
{
    public class CouponOrderConfig : IEntityTypeConfiguration<CouponOrder>
    {
        public void Configure(EntityTypeBuilder<CouponOrder> builder)
        {
            builder.HasKey(co => co.Id);

            builder.Property(co => co.DiscountAmount).HasPrecision(18, 2);
            builder.Property(co => co.CommissionAmount).HasPrecision(18, 2);

            builder.HasIndex(co => co.CouponId);
            builder.HasIndex(co => co.OrderId);
        }
    }
}
