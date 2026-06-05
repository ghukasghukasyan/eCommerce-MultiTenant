using eCommerce.Application.DTOs.Influencers;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces;
using eCommerce.Application.Services.Interfaces.Files;
using eCommerce.Application.Services.Interfaces.Influencers;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Entities.Influencers;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Coupons;
using eCommerce.Domain.Interfaces.Influencers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Web;
using static eCommerce.Domain.Enums.Statuses;
using static eCommerce.Domain.Enums.Types;

namespace eCommerce.Application.Services.Implementations.Influencers
{
    public class InfluencerService(
        IInfluencerRepository influencerRepository,
        IFileService fileService,
        ICouponRepository couponRepository,
        IUserManagement userManagement,
        IRoleManagement roleManagement,
        IEmailService emailService,
        IConfiguration config) : IInfluencerService
    {
        public async Task<ServiceResponse> CreateAsync(CreateInfluencerDTO dto)
        {
            var existingUser = await userManagement.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                return new ServiceResponse(false, "A user with this email already exists.");

            var tempPassword = $"Tmp@{Guid.NewGuid():N}"[..16] + "1!";
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                EmailConfirmed = true
            };

            var result = await userManagement.RegisterAsync(user, tempPassword);
            if (!result.Succeeded)
                return new ServiceResponse(false, result.Errors.FirstOrDefault()?.Description ?? "Registration failed.");

            await roleManagement.AddUserToRole(user, "Influencer");
            await roleManagement.AddUserToRole(user, "User");

            var createdUser = await userManagement.GetByEmailAsync(dto.Email);
            var resetToken = await userManagement.GeneratePasswordResetTokenAsync(createdUser!);

            var influencer = new Influencer
            {
                Id = Guid.NewGuid(),
                UserId = createdUser!.Id,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                DefaultCommissionRate = dto.DefaultCommissionRate,
                Status = InfluencerStatus.Approved,
                RegisteredAt = DateTime.UtcNow
            };

            await influencerRepository.AddAsync(influencer);

            var frontendBase = config["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost";
            var setupLink = $"{frontendBase}/authentication/reset-password?userId={HttpUtility.UrlEncode(createdUser.Id)}&token={HttpUtility.UrlEncode(resetToken)}";
            await emailService.SendPasswordResetAsync(createdUser.Email!, createdUser.FullName, setupLink);

            return new ServiceResponse(true, "Influencer created successfully.");
        }

        public async Task<ServiceResponse> UpdateStatusAsync(UpdateInfluencerDTO request)
        {
            var influencer = await influencerRepository.GetByIdAsync(request.Id);
            if (influencer == null)
                return new ServiceResponse(false, "Influencer not found");

            influencer.FullName = request.FullName;
            influencer.Email = request.Email;
            influencer.PhoneNumber = request.PhoneNumber;
            influencer.DefaultCommissionRate = request.DefaultCommissionRate;

            await influencerRepository.UpdateAsync(influencer);
            return new ServiceResponse(true, "Influencer updated successfully");
        }

        public async Task<ServiceResponse> UpdateInfluencerStatusAsync(UpdateInfluencerStatusDTO request)
        {
            var influencer = await influencerRepository.GetByIdAsync(request.Id);
            if (influencer == null)
                return new ServiceResponse(false, "Influencer not found");

            influencer.Status = request.Status;

            if (request.Status == InfluencerStatus.Rejected)
                influencer.RejectionReason = request.RejectionReason?.Trim();
            else
                influencer.RejectionReason = null;

            // Mark coupon changes BEFORE saving so influencer + coupons commit in one SaveChangesAsync
            if (request.Status == InfluencerStatus.Suspended)
            {
                var coupons = await couponRepository.GetByInfluencerAsync(request.Id);
                foreach (var coupon in coupons)
                {
                    coupon.Status = ActivityStatus.Paused;
                    await couponRepository.UpdateAsync(coupon);
                }
            }
            else if (request.Status == InfluencerStatus.Approved)
            {
                var coupons = await couponRepository.GetByInfluencerAsync(request.Id);
                foreach (var coupon in coupons.Where(c => c.Status == ActivityStatus.Paused))
                {
                    coupon.Status = ActivityStatus.Active;
                    await couponRepository.UpdateAsync(coupon);
                }
            }

            await influencerRepository.UpdateAsync(influencer);

            return new ServiceResponse(true, "Influencer status updated successfully");
        }

        public async Task<IReadOnlyList<GetInfluencerDTO>> GetAllAsync()
        {
            var influencers = await influencerRepository.GetAllAsync();
            return influencers.Select(MapToDTO).ToList();
        }

        public async Task<GetInfluencerDTO?> GetByIdAsync(Guid influencerId)
        {
            var influencer = await influencerRepository.GetByIdAsync(influencerId);
            return influencer is null ? null : MapToDTO(influencer);
        }

        public async Task<GetInfluencerDTO?> GetByUserIdAsync(string userId)
        {
            var influencer = await influencerRepository.GetByIdAsync(userId);
            return influencer is null ? null : MapToDTO(influencer);
        }

        public async Task<InfluencerStatsDTO?> GetMyStatsAsync(string userId)
        {
            var influencer = await influencerRepository.GetByIdAsync(userId);
            if (influencer is null) return null;
            return await BuildStatsAsync(influencer.Id);
        }

        public async Task<InfluencerStatsDTO?> GetStatsByInfluencerIdAsync(Guid influencerId)
        {
            var influencer = await influencerRepository.GetByIdAsync(influencerId);
            if (influencer is null) return null;
            return await BuildStatsAsync(influencerId);
        }

        public async Task<ServiceResponse> RecordPayoutAsync(CreateInfluencerPayoutDTO dto)
        {
            var influencer = await influencerRepository.GetByIdAsync(dto.InfluencerId);
            if (influencer is null)
                return new ServiceResponse(false, "Influencer not found");

            if (dto.Amount <= 0)
                return new ServiceResponse(false, "Payout amount must be greater than zero");

            var payout = new InfluencerPayout
            {
                Id = Guid.NewGuid(),
                InfluencerId = dto.InfluencerId,
                Amount = dto.Amount,
                Note = dto.Note?.Trim(),
                PaidAt = DateTime.UtcNow
            };

            await influencerRepository.AddPayoutAsync(payout);
            return new ServiceResponse(true, "Payout recorded successfully");
        }

        public async Task UploadAvatarAsync(Guid influencerId, IFormFile file)
        {
            var influencer = await influencerRepository.GetByIdAsync(influencerId)
                ?? throw new Exception("Influencer not found");

            var avatarUrl = await fileService.SaveImageAsync(file, FileEntityType.Influencer, influencerId);
            influencer.AvatarUrl = avatarUrl;
            await influencerRepository.UpdateAsync(influencer);
        }

        // ── helpers ──────────────────────────────────────────────────────

        private static GetInfluencerDTO MapToDTO(Domain.Entities.Influencers.Influencer x) => new()
        {
            Id = x.Id,
            FullName = x.FullName,
            Email = x.Email,
            PhoneNumber = x.PhoneNumber,
            InstagramAccountUrl = x.InstagramAccountUrl,
            TikTokAccountUrl = x.TikTokAccountUrl,
            DefaultCommissionRate = x.DefaultCommissionRate,
            Status = x.Status,
            RejectionReason = x.RejectionReason,
            AvatarUrl = x.AvatarUrl,
            RegisteredAt = x.RegisteredAt
        };

        private async Task<InfluencerStatsDTO> BuildStatsAsync(Guid influencerId)
        {
            var coupons = (await couponRepository.GetByInfluencerAsync(influencerId)).ToList();
            var couponOrders = await couponRepository.GetCouponOrdersByInfluencerAsync(influencerId);
            var payoutHistory = await influencerRepository.GetPayoutsByInfluencerAsync(influencerId);

            var couponPayouts = coupons.Select(c =>
            {
                var orders = couponOrders.Where(co => co.CouponId == c.Id).ToList();
                return new CouponPayoutDTO
                {
                    CouponId = c.Id,
                    Code = c.Code,
                    CommissionRate = c.CommissionRate,
                    UsedCount = orders.Count,
                    TotalEarned = orders.Sum(o => o.CommissionAmount)
                };
            }).ToList();

            return new InfluencerStatsDTO
            {
                TotalEarned = couponPayouts.Sum(p => p.TotalEarned),
                TotalPaid = payoutHistory.Sum(p => p.Amount),
                TotalOrders = couponOrders.Count,
                ActiveCoupons = coupons.Count(c => c.Status == ActivityStatus.Active),
                Payouts = couponPayouts,
                PayoutHistory = payoutHistory.Select(p => new GetInfluencerPayoutDTO
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Note = p.Note,
                    PaidAt = p.PaidAt
                }).ToList()
            };
        }
    }
}
