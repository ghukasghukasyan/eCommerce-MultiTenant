using eCommerce.Application.DTOs.Influencers;
using eCommerce.Application.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace eCommerce.Application.Services.Interfaces.Influencers
{
    public interface IInfluencerService
    {
        Task<ServiceResponse> CreateAsync(CreateInfluencerDTO dto);
        Task<IReadOnlyList<GetInfluencerDTO>> GetAllAsync();
        Task<GetInfluencerDTO?> GetByIdAsync(Guid influencerId);
        Task<GetInfluencerDTO?> GetByUserIdAsync(string userId);
        Task<InfluencerStatsDTO?> GetMyStatsAsync(string userId);
        Task<InfluencerStatsDTO?> GetStatsByInfluencerIdAsync(Guid influencerId);
        Task<ServiceResponse> RecordPayoutAsync(CreateInfluencerPayoutDTO dto);
        Task<ServiceResponse> UpdateStatusAsync(UpdateInfluencerDTO request);
        Task<ServiceResponse> UpdateInfluencerStatusAsync(UpdateInfluencerStatusDTO request);
        Task UploadAvatarAsync(Guid influencerId, IFormFile file);
    }
}
