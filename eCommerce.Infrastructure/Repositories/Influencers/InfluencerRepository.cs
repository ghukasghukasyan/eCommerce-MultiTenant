using eCommerce.Domain.Entities.Influencers;
using eCommerce.Domain.Interfaces.Influencers;
using eCommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Infrastructure.Repositories.Influencers
{
    public class InfluencerRepository(ECommerceContext context) : IInfluencerRepository
    {
        public async Task AddAsync(Influencer influencer)
        {
            context.Influencers.Add(influencer);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Influencer influencer)
        {
            context.Influencers.Update(influencer);
            await context.SaveChangesAsync();
        }
        public async Task<IEnumerable<Influencer>> GetAllAsync()
        {
            var result = await context.Influencers
           .OrderByDescending(x => x.RegisteredAt)
           .ToListAsync();

            return result;
        }

        public async Task<Influencer> GetByIdAsync(Guid id)
        {
            return await context.Influencers
             .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Influencer> GetByIdAsync(string id)
        {
            return await context.Influencers
             .FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task<Influencer> GetByEmailAsync(string email)
        {
            return await context.Influencers
             .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<List<Influencer>> GetByIdsAsync(IEnumerable<Guid> ids)
            => await context.Influencers
                .Where(i => ids.Contains(i.Id))
                .ToListAsync();

        public async Task<int> CountByStatusAsync(InfluencerStatus status)
            => await context.Influencers.CountAsync(i => i.Status == status);

        public async Task AddPayoutAsync(InfluencerPayout payout)
        {
            context.InfluencerPayouts.Add(payout);
            await context.SaveChangesAsync();
        }

        public async Task<List<InfluencerPayout>> GetPayoutsByInfluencerAsync(Guid influencerId)
            => await context.InfluencerPayouts
                .Where(p => p.InfluencerId == influencerId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync();
    }
}
