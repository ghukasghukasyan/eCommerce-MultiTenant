using eCommerce.Domain.Entities.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eCommerce.Infrastructure.Data.EntityConfigs.Categories
{
    public class CategoryConfig  : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasOne(c => c.ParentCategory)
                        .WithMany(c => c.Children)
                        .HasForeignKey(c => c.ParentCategoryId)
                        .OnDelete(DeleteBehavior.Restrict);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(150);
        }
    }
}
