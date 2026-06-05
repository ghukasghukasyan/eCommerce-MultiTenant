using eCommerce.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Orders
{
    public class OrderItemConfig : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.UnitPrice)
                   .HasPrecision(18, 2);

            builder.Property(i => i.Quantity)
                   .IsRequired();

            builder.HasOne(i => i.ProductVariant)
                   .WithMany()
                   .HasForeignKey(i => i.ProductVariantId);
        }
    }

}
