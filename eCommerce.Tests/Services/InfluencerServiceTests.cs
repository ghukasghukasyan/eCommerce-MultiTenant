using eCommerce.Application.DTOs.Influencers;
using eCommerce.Application.Services.Implementations.Influencers;
using eCommerce.Application.Services.Interfaces;
using eCommerce.Application.Services.Interfaces.Files;
using eCommerce.Domain.Entities.Coupons;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Entities.Influencers;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Coupons;
using eCommerce.Domain.Interfaces.Influencers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using static eCommerce.Domain.Enums.Statuses;
using static eCommerce.Domain.Enums.Types;

namespace eCommerce.Tests.Services;

public class InfluencerServiceTests
{
    private readonly Mock<IInfluencerRepository>  _repo             = new();
    private readonly Mock<IFileService>            _fileService      = new();
    private readonly Mock<ICouponRepository>       _couponRepository = new();
    private readonly Mock<IUserManagement>         _userManagement   = new();
    private readonly Mock<IRoleManagement>         _roleManagement   = new();
    private readonly Mock<IEmailService>           _emailService     = new();
    private readonly Mock<IConfiguration>          _config           = new();

    private InfluencerService CreateService() => new(
        _repo.Object,
        _fileService.Object,
        _couponRepository.Object,
        _userManagement.Object,
        _roleManagement.Object,
        _emailService.Object,
        _config.Object);

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ReturnsFailure_WhenUserAlreadyExists()
    {
        var existing = new AppUser { Id = "u1", Email = "taken@test.com" };
        _userManagement.Setup(x => x.GetByEmailAsync("taken@test.com")).ReturnsAsync(existing);

        var result = await CreateService().CreateAsync(new CreateInfluencerDTO
        {
            Email       = "taken@test.com",
            FullName    = "Test",
            PhoneNumber = "+37400000000"
        });

        Assert.False(result.Success);
        Assert.Equal("A user with this email already exists.", result.Message);
        _repo.Verify(x => x.AddAsync(It.IsAny<Influencer>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ReturnsFailure_WhenRegistrationFails()
    {
        _userManagement.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((AppUser?)null);
        _userManagement.Setup(x => x.RegisterAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Weak password" }));

        var result = await CreateService().CreateAsync(new CreateInfluencerDTO
        {
            Email       = "new@test.com",
            FullName    = "Test",
            PhoneNumber = "+37400000000"
        });

        Assert.False(result.Success);
        Assert.Equal("Weak password", result.Message);
        _repo.Verify(x => x.AddAsync(It.IsAny<Influencer>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CreatesInfluencerAndSendsSetupEmail_WhenSuccessful()
    {
        var createdUser = new AppUser
        {
            Id       = "user-1",
            Email    = "inf@test.com",
            FullName = "Influence Smith"
        };

        _userManagement.Setup(x => x.GetByEmailAsync("inf@test.com"))
            .ReturnsAsync((AppUser?)null);
        _userManagement.Setup(x => x.RegisterAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagement.Setup(x => x.AddUserToRole(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManagement.SetupSequence(x => x.GetByEmailAsync("inf@test.com"))
            .ReturnsAsync((AppUser?)null)      // first call: existence check
            .ReturnsAsync(createdUser);         // second call: after registration
        _userManagement.Setup(x => x.GeneratePasswordResetTokenAsync(createdUser))
            .ReturnsAsync("reset-token-abc");
        _config.Setup(x => x["Frontend:BaseUrl"]).Returns("https://mystore.com");

        Influencer? savedInfluencer = null;
        _repo.Setup(x => x.AddAsync(It.IsAny<Influencer>()))
            .Callback<Influencer>(i => savedInfluencer = i);

        string? sentEmail = null;
        string? sentLink  = null;
        _emailService
            .Setup(x => x.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string, string>((email, _, link) => { sentEmail = email; sentLink = link; })
            .Returns(Task.CompletedTask);

        var result = await CreateService().CreateAsync(new CreateInfluencerDTO
        {
            Email                 = "inf@test.com",
            FullName              = "Influence Smith",
            PhoneNumber           = "+37400000000",
            DefaultCommissionRate = 15
        });

        Assert.True(result.Success);
        Assert.Equal("Influencer created successfully.", result.Message);

        Assert.NotNull(savedInfluencer);
        Assert.Equal("inf@test.com", savedInfluencer!.Email);
        Assert.Equal(InfluencerStatus.Approved, savedInfluencer.Status);

        _emailService.Verify(
            x => x.SendPasswordResetAsync("inf@test.com", "Influence Smith", It.IsAny<string>()),
            Times.Once);

        Assert.Equal("inf@test.com", sentEmail);
        Assert.Contains("reset-token-abc", sentLink);
        Assert.StartsWith("https://mystore.com", sentLink);
    }

    // ── UpdateStatusAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFailure_WhenInfluencerMissing()
    {
        var dto = new UpdateInfluencerDTO { Id = Guid.NewGuid(), FullName = "X", Email = "x@y.com", DefaultCommissionRate = 10 };
        _repo.Setup(x => x.GetByIdAsync(dto.Id)).ReturnsAsync((Influencer?)null);

        var result = await CreateService().UpdateStatusAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Influencer not found", result.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesInfluencer_WhenFound()
    {
        var influencer = new Influencer
        {
            Id = Guid.NewGuid(),
            FullName = "Old",
            Email = "old@test.com",
            PhoneNumber = "+1000000000",
            DefaultCommissionRate = 5
        };
        var dto = new UpdateInfluencerDTO
        {
            Id = influencer.Id,
            FullName = "New",
            Email = "new@test.com",
            PhoneNumber = "+12223334444",
            DefaultCommissionRate = 20
        };

        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);

        var result = await CreateService().UpdateStatusAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("New", influencer.FullName);
        Assert.Equal("new@test.com", influencer.Email);
        Assert.Equal("+12223334444", influencer.PhoneNumber);
        Assert.Equal(20, influencer.DefaultCommissionRate);
        _repo.Verify(x => x.UpdateAsync(influencer), Times.Once);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_MapsInfluencers()
    {
        var now = DateTime.UtcNow;
        _repo.Setup(x => x.GetAllAsync()).ReturnsAsync(
        [
            new Influencer
            {
                Id = Guid.NewGuid(),
                FullName = "Jane Doe",
                Email = "jane@example.com",
                DefaultCommissionRate = 15,
                RegisteredAt = now
            }
        ]);

        var result = await CreateService().GetAllAsync();

        var dto = Assert.Single(result);
        Assert.Equal("Jane Doe", dto.FullName);
        Assert.Equal("jane@example.com", dto.Email);
        Assert.Equal(15, dto.DefaultCommissionRate);
        Assert.Equal(now, dto.RegisteredAt);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Influencer?)null);

        var result = await CreateService().GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── UpdateInfluencerStatusAsync ───────────────────────────────────────────

    [Fact]
    public async Task UpdateInfluencerStatusAsync_ReturnsFailure_WhenNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Influencer?)null);

        var result = await CreateService().UpdateInfluencerStatusAsync(new UpdateInfluencerStatusDTO
        {
            Id = Guid.NewGuid(),
            Status = InfluencerStatus.Approved
        });

        Assert.False(result.Success);
        Assert.Equal("Influencer not found", result.Message);
    }

    [Fact]
    public async Task UpdateInfluencerStatusAsync_UpdatesStatus_WhenFound()
    {
        var influencer = new Influencer { Id = Guid.NewGuid() };
        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);
        _couponRepository.Setup(x => x.GetByInfluencerAsync(influencer.Id)).ReturnsAsync(new List<Coupon>());

        var result = await CreateService().UpdateInfluencerStatusAsync(new UpdateInfluencerStatusDTO
        {
            Id = influencer.Id,
            Status = InfluencerStatus.Approved
        });

        Assert.True(result.Success);
        Assert.Equal(InfluencerStatus.Approved, influencer.Status);
        _repo.Verify(x => x.UpdateAsync(influencer), Times.Once);
    }

    [Fact]
    public async Task UpdateInfluencerStatusAsync_SetsRejectionReason_WhenRejected()
    {
        var influencer = new Influencer { Id = Guid.NewGuid() };
        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);

        await CreateService().UpdateInfluencerStatusAsync(new UpdateInfluencerStatusDTO
        {
            Id = influencer.Id,
            Status = InfluencerStatus.Rejected,
            RejectionReason = "  Does not meet criteria  "
        });

        Assert.Equal(InfluencerStatus.Rejected, influencer.Status);
        Assert.Equal("Does not meet criteria", influencer.RejectionReason);
        _repo.Verify(x => x.UpdateAsync(influencer), Times.Once);
    }

    [Fact]
    public async Task UpdateInfluencerStatusAsync_ClearsRejectionReason_WhenApproved()
    {
        var influencer = new Influencer { Id = Guid.NewGuid(), RejectionReason = "Old reason" };
        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);
        _couponRepository.Setup(x => x.GetByInfluencerAsync(influencer.Id)).ReturnsAsync(new List<Coupon>());

        await CreateService().UpdateInfluencerStatusAsync(new UpdateInfluencerStatusDTO
        {
            Id = influencer.Id,
            Status = InfluencerStatus.Approved
        });

        Assert.Null(influencer.RejectionReason);
        _repo.Verify(x => x.UpdateAsync(influencer), Times.Once);
    }

    [Fact]
    public async Task UpdateInfluencerStatusAsync_PausesCoupons_WhenSuspended()
    {
        var influencer = new Influencer { Id = Guid.NewGuid() };
        var coupon1 = new Coupon { Id = Guid.NewGuid(), Status = ActivityStatus.Active };
        var coupon2 = new Coupon { Id = Guid.NewGuid(), Status = ActivityStatus.Active };

        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);
        _couponRepository.Setup(x => x.GetByInfluencerAsync(influencer.Id))
                         .ReturnsAsync(new List<Coupon> { coupon1, coupon2 });

        await CreateService().UpdateInfluencerStatusAsync(new UpdateInfluencerStatusDTO
        {
            Id = influencer.Id,
            Status = InfluencerStatus.Suspended
        });

        Assert.Equal(ActivityStatus.Paused, coupon1.Status);
        Assert.Equal(ActivityStatus.Paused, coupon2.Status);
        _couponRepository.Verify(x => x.UpdateAsync(It.IsAny<Coupon>()), Times.Exactly(2));
        // Coupons and influencer saved together in one call to influencerRepository.UpdateAsync
        _couponRepository.Verify(x => x.SaveAsync(), Times.Never);
        _repo.Verify(x => x.UpdateAsync(influencer), Times.Once);
    }

    [Fact]
    public async Task UpdateInfluencerStatusAsync_ReactivatesPausedCoupons_WhenApproved()
    {
        var influencer = new Influencer { Id = Guid.NewGuid() };
        var paused = new Coupon { Id = Guid.NewGuid(), Status = ActivityStatus.Paused };
        var active = new Coupon { Id = Guid.NewGuid(), Status = ActivityStatus.Active };

        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);
        _couponRepository.Setup(x => x.GetByInfluencerAsync(influencer.Id))
                         .ReturnsAsync(new List<Coupon> { paused, active });

        await CreateService().UpdateInfluencerStatusAsync(new UpdateInfluencerStatusDTO
        {
            Id = influencer.Id,
            Status = InfluencerStatus.Approved
        });

        Assert.Equal(ActivityStatus.Active, paused.Status);   // was paused, now active
        Assert.Equal(ActivityStatus.Active, active.Status);   // was already active, unchanged
        _couponRepository.Verify(x => x.UpdateAsync(paused), Times.Once);
        _couponRepository.Verify(x => x.UpdateAsync(active), Times.Never);
        // Coupons and influencer saved together in one call to influencerRepository.UpdateAsync
        _couponRepository.Verify(x => x.SaveAsync(), Times.Never);
        _repo.Verify(x => x.UpdateAsync(influencer), Times.Once);
    }

    // ── UploadAvatarAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UploadAvatarAsync_Throws_WhenInfluencerMissing()
    {
        _repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Influencer?)null);

        var ex = await Assert.ThrowsAsync<Exception>(() => CreateService().UploadAvatarAsync(Guid.NewGuid(), Mock.Of<IFormFile>()));

        Assert.Equal("Influencer not found", ex.Message);
    }

    [Fact]
    public async Task UploadAvatarAsync_StoresAvatarUrl_AndUpdatesInfluencer()
    {
        var influencerId = Guid.NewGuid();
        var influencer = new Influencer { Id = influencerId };
        var file = Mock.Of<IFormFile>();

        _repo.Setup(x => x.GetByIdAsync(influencerId)).ReturnsAsync(influencer);
        _fileService
            .Setup(x => x.SaveImageAsync(file, FileEntityType.Influencer, influencerId))
            .ReturnsAsync("/uploads/influencer.png");

        await CreateService().UploadAvatarAsync(influencerId, file);

        Assert.Equal("/uploads/influencer.png", influencer.AvatarUrl);
        _repo.Verify(x => x.UpdateAsync(influencer), Times.Once);
    }

    // ── GetByUserIdAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserIdAsync_ReturnsNull_WhenNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Influencer?)null);

        var result = await CreateService().GetByUserIdAsync("user-99");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsMappedDto_WhenFound()
    {
        var influencer = new Influencer
        {
            Id       = Guid.NewGuid(),
            FullName = "Bob",
            Email    = "bob@x.com",
            DefaultCommissionRate = 12,
            RegisteredAt = DateTime.UtcNow
        };

        _repo.Setup(x => x.GetByIdAsync("user-1")).ReturnsAsync(influencer);

        var result = await CreateService().GetByUserIdAsync("user-1");

        Assert.NotNull(result);
        Assert.Equal("Bob", result!.FullName);
        Assert.Equal("bob@x.com", result.Email);
    }

    // ── RecordPayoutAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RecordPayoutAsync_ReturnsFailure_WhenInfluencerNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Influencer?)null);

        var result = await CreateService().RecordPayoutAsync(new CreateInfluencerPayoutDTO
        {
            InfluencerId = Guid.NewGuid(),
            Amount       = 500m
        });

        Assert.False(result.Success);
        Assert.Equal("Influencer not found", result.Message);
        _repo.Verify(x => x.AddPayoutAsync(It.IsAny<InfluencerPayout>()), Times.Never);
    }

    [Fact]
    public async Task RecordPayoutAsync_ReturnsFailure_WhenAmountIsZero()
    {
        var influencer = new Influencer { Id = Guid.NewGuid() };
        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);

        var result = await CreateService().RecordPayoutAsync(new CreateInfluencerPayoutDTO
        {
            InfluencerId = influencer.Id,
            Amount       = 0m
        });

        Assert.False(result.Success);
        Assert.Equal("Payout amount must be greater than zero", result.Message);
        _repo.Verify(x => x.AddPayoutAsync(It.IsAny<InfluencerPayout>()), Times.Never);
    }

    [Fact]
    public async Task RecordPayoutAsync_ReturnsFailure_WhenAmountIsNegative()
    {
        var influencer = new Influencer { Id = Guid.NewGuid() };
        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);

        var result = await CreateService().RecordPayoutAsync(new CreateInfluencerPayoutDTO
        {
            InfluencerId = influencer.Id,
            Amount       = -100m
        });

        Assert.False(result.Success);
    }

    [Fact]
    public async Task RecordPayoutAsync_PersistsPayout_WhenValid()
    {
        var influencer = new Influencer { Id = Guid.NewGuid() };
        _repo.Setup(x => x.GetByIdAsync(influencer.Id)).ReturnsAsync(influencer);

        InfluencerPayout? saved = null;
        _repo.Setup(x => x.AddPayoutAsync(It.IsAny<InfluencerPayout>()))
            .Callback<InfluencerPayout>(p => saved = p);

        var result = await CreateService().RecordPayoutAsync(new CreateInfluencerPayoutDTO
        {
            InfluencerId = influencer.Id,
            Amount       = 1500m,
            Note         = "  Q1 bonus  "
        });

        Assert.True(result.Success);
        Assert.Equal("Payout recorded successfully", result.Message);
        Assert.NotNull(saved);
        Assert.Equal(influencer.Id, saved!.InfluencerId);
        Assert.Equal(1500m, saved.Amount);
        Assert.Equal("Q1 bonus", saved.Note);
    }

    // ── GetMyStatsAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyStatsAsync_ReturnsNull_WhenInfluencerNotFound()
    {
        _repo.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync((Influencer?)null);

        var result = await CreateService().GetMyStatsAsync("user-999");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMyStatsAsync_ReturnsStats_WhenFound()
    {
        var influencerId = Guid.NewGuid();
        var influencer   = new Influencer { Id = influencerId };

        _repo.Setup(x => x.GetByIdAsync("user-1")).ReturnsAsync(influencer);

        var couponId = Guid.NewGuid();
        _couponRepository.Setup(x => x.GetByInfluencerAsync(influencerId)).ReturnsAsync(
        [
            new Coupon { Id = couponId, Code = "SAVE10", CommissionRate = 10, Status = ActivityStatus.Active }
        ]);
        _couponRepository.Setup(x => x.GetCouponOrdersByInfluencerAsync(influencerId)).ReturnsAsync(
        [
            new CouponOrder { CouponId = couponId, CommissionAmount = 200m },
            new CouponOrder { CouponId = couponId, CommissionAmount = 300m }
        ]);
        _repo.Setup(x => x.GetPayoutsByInfluencerAsync(influencerId)).ReturnsAsync(
        [
            new InfluencerPayout { Amount = 150m, PaidAt = DateTime.UtcNow }
        ]);

        var result = await CreateService().GetMyStatsAsync("user-1");

        Assert.NotNull(result);
        Assert.Equal(500m, result!.TotalEarned);
        Assert.Equal(150m, result.TotalPaid);
        Assert.Equal(2, result.TotalOrders);
        Assert.Equal(1, result.ActiveCoupons);
        Assert.Single(result.Payouts);
        Assert.Equal("SAVE10", result.Payouts[0].Code);
        Assert.Equal(500m, result.Payouts[0].TotalEarned);
    }
}
