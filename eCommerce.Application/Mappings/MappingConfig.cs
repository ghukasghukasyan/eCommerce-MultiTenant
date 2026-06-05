using AutoMapper;
using eCommerce.Application.DTOs.Categories;
using eCommerce.Application.DTOs.Identity;
using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Products;
using eCommerce.Application.DTOs.Products.Variants;
using eCommerce.Domain.Entities.Categories;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Entities.Products;

namespace eCommerce.Application.Mappings
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<CreateCategoryDTO, Category>();
            CreateMap<UpdateCategoryDTO, Category>();
            CreateMap<Category, GetCategoryDTO>();

            CreateMap<CreateProductDTO, Product>();
            CreateMap<UpdateProductDTO, Product>();
            CreateMap<Product, GetProductDTO>()
           .ForMember(dest => dest.DefaultVariantId,
                opt => opt.MapFrom(src =>
                    src.Variants
                        .Where(v => v.IsActive)
                        .Select(v => v.Id)
                        .FirstOrDefault()))
           .ForMember(dest => dest.Stock,
               opt => opt.MapFrom(src =>
                   src.Variants
                       .Where(v => v.IsActive)
                       .Sum(v => v.StockQuantity)))
           .ForMember(dest => dest.Price,
               opt => opt.MapFrom(src =>
                   src.Variants
                       .Where(v => v.IsActive)
                       .Select(v => v.Price)
                       .DefaultIfEmpty(0m)
                       .Min()))
           .ForMember(dest => dest.CreatedAt,
               opt => opt.MapFrom(src => src.CreatedAt))
           .ForMember(dest => dest.Images,
               opt => opt.MapFrom(src => src.Images))
           .ForMember(dest => dest.Variants,
               opt => opt.MapFrom(src => src.Variants
                   .Select(v => new VariantDTO
                   {
                       VariantId = v.Id,
                       Attributes = v.AttributeValues
                           .ToDictionary(av => av.VariantAttribute.Name, av => av.Value),
                       Price = v.Price,
                       Stock = v.StockQuantity,
                       IsActive = v.IsActive
                   })
                   .ToList()));

            CreateMap<ProductImage, ProductImageDTO>();

            CreateMap<RegisterUserDTO, AppUser>();
            CreateMap<LoginUserDTO, AppUser>();

            CreateMap<PaymentMethod, GetPaymentMethodDTO>();
            CreateMap<CreateOrderDTO, Order>();
        }
    }
}
