using eCommerce.Domain.Entities.Influencers;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Domain.Interfaces.Influencers
{
    public interface IInfluencerRepository
    {
        Task AddAsync(Influencer influencer);
        Task UpdateAsync(Influencer influencer);
        Task<Influencer> GetByIdAsync(Guid id);
        Task<Influencer> GetByIdAsync(string id);
        Task<Influencer> GetByEmailAsync(string email);
        Task<IEnumerable<Influencer>> GetAllAsync();
        Task<List<Influencer>> GetByIdsAsync(IEnumerable<Guid> ids);
        Task<int> CountByStatusAsync(InfluencerStatus status);

        Task AddPayoutAsync(InfluencerPayout payout);
        Task<List<InfluencerPayout>> GetPayoutsByInfluencerAsync(Guid influencerId);
    }
}
