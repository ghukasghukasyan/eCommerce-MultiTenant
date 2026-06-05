using eCommerce.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Products
{
    public class VariantAttributeValueConfig : IEntityTypeConfiguration<VariantAttributeValue>
    {
        public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
        {
            builder.Property(v => v.Value)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasOne(v => v.ProductVariant)
                   .WithMany(pv => pv.AttributeValues)
                   .HasForeignKey(v => v.ProductVariantId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(v => v.VariantAttribute)
                   .WithMany()
                   .HasForeignKey(v => v.VariantAttributeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(v => new
            {
                v.ProductVariantId,
                v.VariantAttributeId
            }).IsUnique();
        }
    }
}
