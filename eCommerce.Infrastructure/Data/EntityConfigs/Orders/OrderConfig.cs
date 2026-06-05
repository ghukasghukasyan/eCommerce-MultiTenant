using eCommerce.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Orders
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(o => o.ShippingDetail);

            builder.HasKey(o => o.Id);

            builder.Property(o => o.UserId)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(o => o.TotalAmount)
                   .HasPrecision(18, 2);

            builder.Property(o => o.Status)
                   .IsRequired();

            builder.HasMany(o => o.Items)
                   .WithOne(i => i.Order)
                   .HasForeignKey(i => i.OrderId);

            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.CreatedAt);
        }
    }

}
