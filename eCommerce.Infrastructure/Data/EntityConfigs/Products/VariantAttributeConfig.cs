using eCommerce.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Products
{
    public class VariantAttributeConfig : IEntityTypeConfiguration<VariantAttribute>
    {
        public void Configure(EntityTypeBuilder<VariantAttribute> builder)
        {
            builder.Property(a => a.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(a => a.Name)
                   .IsUnique();
        }
    }
}
