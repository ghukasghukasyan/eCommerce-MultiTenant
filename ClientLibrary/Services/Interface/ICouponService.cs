using ClientLibrary.Models.Coupons;
using ClientLibrary.Models.Responses;

namespace ClientLibrary.Services.Interface
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
