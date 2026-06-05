using eCommerce.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Products
{
    public class ProductVariantConfig : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.Property(v => v.Sku)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(v => v.Sku)
                   .IsUnique();

            builder.Property(v => v.Price)
                   .HasPrecision(18, 2);

            builder.HasOne(v => v.Product)
                   .WithMany(p => p.Variants)
                   .HasForeignKey(v => v.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(v => v.ProductId);
        }
    }
}
