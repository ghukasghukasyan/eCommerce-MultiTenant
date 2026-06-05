using ClientLibrary.Models.Influencers;
using ClientLibrary.Models.Responses;
using Microsoft.AspNetCore.Http;

namespace ClientLibrary.Services.Interface
{
    public interface IInfluencerService
    {
        Task<ServiceResponse<Guid>> CreateAsync(CreateInfluencerDTO request);
        Task<ServiceResponse<Guid>> UpdateAsync(UpdateInfluencerDTO request);
        Task<ServiceResponse<Guid>> UpdateStatusAsync(UpdateInfluencerStatusDTO request);
        Task<IReadOnlyList<GetInfluencerDTO>> GetAllAsync();
        Task<GetInfluencerDTO> GetByIdAsync(Guid influencerId);
        Task<GetInfluencerDTO> GetMeAsync();
        Task<InfluencerStatsDTO?> GetStatsAsync();
        Task<InfluencerStatsDTO?> GetStatsByIdAsync(Guid influencerId);
        Task<ServiceResponse> RecordPayoutAsync(Guid influencerId, decimal amount, string? note);
        Task UploadAvatarAsync(Guid influencerId, IFormFile file);
    }
}
