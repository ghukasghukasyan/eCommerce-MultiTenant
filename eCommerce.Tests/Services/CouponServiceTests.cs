using eCommerce.Application.DTOs.Coupons;
using eCommerce.Application.Services.Implementations.Coupons;
using eCommerce.Domain.Entities.Coupons;
using eCommerce.Domain.Interfaces.Coupons;
using eCommerce.Domain.Interfaces.Influencers;
using Moq;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Tests.Services;

public class CouponServiceTests
{
    private readonly Mock<ICouponRepository> _couponRepo = new();
    private readonly Mock<IInfluencerRepository> _influencerRepo = new();

    private CouponService CreateService() => new(_couponRepo.Object, _influencerRepo.Object);

    // ── ValidateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateAsync_ReturnsInvalid_WhenCouponNotFound()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync("MISS")).ReturnsAsync((Coupon?)null);

        var result = await CreateService().ValidateAsync(new ValidateCouponDTO { Code = "MISS", OrderTotal = 100 });

        Assert.False(result.IsValid);
        Assert.Equal("Coupon not found.", result.Message);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalid_WhenCouponPaused()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync("PAUSED")).ReturnsAsync(new Coupon
        {
            Code = "PAUSED",
            Status = ActivityStatus.Paused,
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10
        });

        var result = await CreateService().ValidateAsync(new ValidateCouponDTO { Code = "PAUSED", OrderTotal = 100 });

        Assert.False(result.IsValid);
        Assert.Equal("This coupon is no longer active.", result.Message);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsInvalid_WhenCouponExpired()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync("OLD")).ReturnsAsync(new Coupon
        {
            Code = "OLD",
            Status = ActivityStatus.Active,
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10
        });

        var result = await CreateService().ValidateAsync(new ValidateCouponDTO { Code = "OLD", OrderTotal = 100 });

        Assert.False(result.IsValid);
        Assert.Equal("This coupon has expired.", result.Message);
    }

    [Fact]
    public async Task ValidateAsync_AppliesPercentageDiscount()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync("PCT20")).ReturnsAsync(new Coupon
        {
            Id = Guid.NewGuid(),
            Code = "PCT20",
            Status = ActivityStatus.Active,
            DiscountType = DiscountType.Percentage,
            DiscountValue = 20
        });

        var result = await CreateService().ValidateAsync(new ValidateCouponDTO { Code = "PCT20", OrderTotal = 500 });

        Assert.True(result.IsValid);
        Assert.Equal(100m, result.DiscountAmount);
        Assert.Equal(400m, result.DiscountedTotal);
    }

    [Fact]
    public async Task ValidateAsync_AppliesFixedDiscount()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync("FIX50")).ReturnsAsync(new Coupon
        {
            Id = Guid.NewGuid(),
            Code = "FIX50",
            Status = ActivityStatus.Active,
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 50
        });

        var result = await CreateService().ValidateAsync(new ValidateCouponDTO { Code = "FIX50", OrderTotal = 200 });

        Assert.True(result.IsValid);
        Assert.Equal(50m, result.DiscountAmount);
        Assert.Equal(150m, result.DiscountedTotal);
    }

    [Fact]
    public async Task ValidateAsync_FixedDiscount_CappedAtOrderTotal()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync("BIG")).ReturnsAsync(new Coupon
        {
            Id = Guid.NewGuid(),
            Code = "BIG",
            Status = ActivityStatus.Active,
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 999
        });

        var result = await CreateService().ValidateAsync(new ValidateCouponDTO { Code = "BIG", OrderTotal = 100 });

        Assert.True(result.IsValid);
        Assert.Equal(100m, result.DiscountAmount);
        Assert.Equal(0m, result.DiscountedTotal);
    }

    [Fact]
    public async Task ValidateAsync_StillValid_WhenNoExpirySet()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync("FOREVER")).ReturnsAsync(new Coupon
        {
            Id = Guid.NewGuid(),
            Code = "FOREVER",
            Status = ActivityStatus.Active,
            ExpiryDate = null,
            DiscountType = DiscountType.Percentage,
            DiscountValue = 5
        });

        var result = await CreateService().ValidateAsync(new ValidateCouponDTO { Code = "FOREVER", OrderTotal = 1000 });

        Assert.True(result.IsValid);
    }

    // ── CalculateDiscount ──────────────────────────────────────────────────────

    [Theory]
    [InlineData(10d, 200d, 20d)]
    [InlineData(25d, 400d, 100d)]
    [InlineData(100d, 300d, 300d)]
    public void CalculateDiscount_Percentage_IsCorrect(double rate, double total, double expected)
    {
        var coupon = new Coupon { DiscountType = DiscountType.Percentage, DiscountValue = (int)rate };
        Assert.Equal((decimal)expected, CouponService.CalculateDiscount(coupon, (decimal)total));
    }

    [Theory]
    [InlineData(50d, 200d, 50d)]
    [InlineData(500d, 100d, 100d)]  // capped
    [InlineData(0d, 200d, 0d)]
    public void CalculateDiscount_FixedAmount_IsCorrect(double value, double total, double expected)
    {
        var coupon = new Coupon { DiscountType = DiscountType.FixedAmount, DiscountValue = (int)value };
        Assert.Equal((decimal)expected, CouponService.CalculateDiscount(coupon, (decimal)total));
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ReturnsFailure_WhenCodeAlreadyExists()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync(new Coupon { Id = Guid.NewGuid() });

        var result = await CreateService().CreateAsync(new CreateCouponDTO
        {
            Code = "TAKEN",
            InfluencerId = Guid.NewGuid(),
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10
        });

        Assert.False(result.Success);
        Assert.Equal("Coupon code already exists.", result.Message);
        _couponRepo.Verify(x => x.AddAsync(It.IsAny<Coupon>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_NormalizesCode_ToUppercaseTrimmed()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync((Coupon?)null);

        Coupon? saved = null;
        _couponRepo.Setup(x => x.AddAsync(It.IsAny<Coupon>()))
            .Callback<Coupon>(c => saved = c);

        await CreateService().CreateAsync(new CreateCouponDTO
        {
            Code = "  summer20  ",
            InfluencerId = Guid.NewGuid(),
            DiscountType = DiscountType.Percentage,
            DiscountValue = 20
        });

        Assert.NotNull(saved);
        Assert.Equal("SUMMER20", saved!.Code);
    }

    [Fact]
    public async Task CreateAsync_Succeeds_WithCorrectDefaults()
    {
        _couponRepo.Setup(x => x.GetByCodeAsync(It.IsAny<string>())).ReturnsAsync((Coupon?)null);

        var result = await CreateService().CreateAsync(new CreateCouponDTO
        {
            Code = "NEW10",
            InfluencerId = Guid.NewGuid(),
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 10,
            CommissionRate = 5
        });

        Assert.True(result.Success);
        _couponRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenCouponNotFound()
    {
        _couponRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Coupon?)null);

        var result = await CreateService().UpdateAsync(new UpdateCouponDTO { Id = Guid.NewGuid(), Code = "X" });

        Assert.False(result.Success);
        Assert.Equal("Coupon not found.", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenCodeTakenByDifferentCoupon()
    {
        var id = Guid.NewGuid();
        var conflictId = Guid.NewGuid();

        _couponRepo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(new Coupon { Id = id, Code = "OLD" });
        _couponRepo.Setup(x => x.GetByCodeAsync("TAKEN")).ReturnsAsync(new Coupon { Id = conflictId, Code = "TAKEN" });

        var result = await CreateService().UpdateAsync(new UpdateCouponDTO { Id = id, Code = "TAKEN" });

        Assert.False(result.Success);
        Assert.Equal("Coupon code already exists.", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WhenCodeBelongsToSameCoupon()
    {
        var id = Guid.NewGuid();
        var coupon = new Coupon { Id = id, Code = "MINE", DiscountType = DiscountType.Percentage, DiscountValue = 10 };

        _couponRepo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(coupon);
        _couponRepo.Setup(x => x.GetByCodeAsync("MINE")).ReturnsAsync(coupon);

        var result = await CreateService().UpdateAsync(new UpdateCouponDTO
        {
            Id = id,
            Code = "MINE",
            DiscountType = DiscountType.FixedAmount,
            DiscountValue = 99,
            CommissionRate = 8
        });

        Assert.True(result.Success);
        Assert.Equal(DiscountType.FixedAmount, coupon.DiscountType);
        Assert.Equal(99, coupon.DiscountValue);
        _couponRepo.Verify(x => x.UpdateAsync(coupon), Times.Once);
        _couponRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    // ── UpdateStatusAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFailure_WhenNotFound()
    {
        _couponRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Coupon?)null);

        var result = await CreateService().UpdateStatusAsync(new UpdateCouponStatusDTO { Id = Guid.NewGuid() });

        Assert.False(result.Success);
        Assert.Equal("Coupon not found.", result.Message);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatus_WhenFound()
    {
        var coupon = new Coupon { Id = Guid.NewGuid(), Status = ActivityStatus.Active };
        _couponRepo.Setup(x => x.GetByIdAsync(coupon.Id)).ReturnsAsync(coupon);

        var result = await CreateService().UpdateStatusAsync(new UpdateCouponStatusDTO
        {
            Id = coupon.Id,
            Status = ActivityStatus.Paused
        });

        Assert.True(result.Success);
        Assert.Equal(ActivityStatus.Paused, coupon.Status);
        _couponRepo.Verify(x => x.UpdateAsync(coupon), Times.Once);
        _couponRepo.Verify(x => x.SaveAsync(), Times.Once);
    }
}
