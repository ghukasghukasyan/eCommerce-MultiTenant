using eCommerce.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Products
{
    public class ProductImageConfig : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
           builder.HasKey(x => x.Id);

            builder.ToTable("ProductImages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.DisplayOrder)
                   .IsRequired();

            builder.Property(x => x.IsMain)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();
        }
    }
}
