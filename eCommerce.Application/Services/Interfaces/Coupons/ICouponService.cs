using eCommerce.Application.DTOs.Coupons;
using eCommerce.Application.DTOs.Responses;

namespace eCommerce.Application.Services.Interfaces.Coupons
{
    public interface ICouponService
    {
        Task<List<GetCouponDTO>> GetAllAsync();
        Task<List<GetCouponDTO>> GetByInfluencerAsync(Guid influencerId);
        Task<ServiceResponse<Guid>> CreateAsync(CreateCouponDTO dto);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateCouponDTO dto);
        Task<ServiceResponse<Guid>> UpdateStatusAsync(UpdateCouponStatusDTO dto);
        Task<CouponValidationResultDTO> ValidateAsync(ValidateCouponDTO dto);
    }
}
