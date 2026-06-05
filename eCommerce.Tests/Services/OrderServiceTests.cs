using eCommerce.Application.DTOs.Addresses;
using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.Services.Implementations.Orders;
using eCommerce.Application.Services.Interfaces;
using eCommerce.Application.Services.Interfaces.Notifications;
using eCommerce.Application.Services.Interfaces.Orders;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Entities.Orders;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Coupons;
using eCommerce.Domain.Interfaces.Orders;
using eCommerce.Domain.Interfaces.Products;
using Moq;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository>           _orderRepository      = new();
    private readonly Mock<IVariantRepository>         _variantRepository    = new();
    private readonly Mock<IPaymentMethodService>      _paymentMethodService = new();
    private readonly Mock<IUserManagement>            _userManagement       = new();
    private readonly Mock<ICouponRepository>          _couponRepository     = new();
    private readonly Mock<IOrderNotificationService>  _notificationService  = new();
    private readonly Mock<IUnitOfWork>                _unitOfWork           = new();

    private OrderService CreateService()
    {
        _unitOfWork
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(op => op());
        return new(
            _orderRepository.Object,
            _variantRepository.Object,
            _paymentMethodService.Object,
            _userManagement.Object,
            _couponRepository.Object,
            _notificationService.Object,
            _unitOfWork.Object);
    }

    // shared helpers
    private static CreateOrderDTO ValidRequest(Guid? paymentMethodId = null, string? couponCode = null) => new()
    {
        PaymentMethodId = paymentMethodId ?? Guid.NewGuid(),
        CouponCode = couponCode,
        ShippingDetail = new ShippingDetailDTO
        {
            FullName = "John Doe",
            PhoneNumber = "+37400000000",
            City = "Yerevan",
            Address = "Main St",
            PostalCode = "0001"
        },
        Items = [new CreateOrderItemDTO { VariantId = Guid.NewGuid(), Quantity = 1 }]
    };

    private void SetupValidUser(string userId = "user-1")
        => _userManagement.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(new AppUser { Id = userId, FullName = "John Doe", Email = "john@x.com" });

    private void SetupPaymentMethod(Guid methodId)
        => _paymentMethodService.Setup(x => x.GetMethodsAsync())
            .ReturnsAsync([new GetPaymentMethodDTO { Id = methodId, Name = "Cash" }]);

    private ProductVariant SetupVariant(Guid variantId, decimal price = 5000, int stock = 10)
    {
        var variant = new ProductVariant { Id = variantId, Price = price, StockQuantity = stock, IsActive = true };
        _variantRepository.Setup(x => x.GetByIdAsync(variantId)).ReturnsAsync(variant);
        _variantRepository.Setup(x => x.DecrementStockAsync(variantId, It.IsAny<int>())).ReturnsAsync(true);
        return variant;
    }

    // ── Guard: empty items ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_ReturnsFailure_WhenItemsEmpty()
    {
        var request = new CreateOrderDTO
        {
            PaymentMethodId = Guid.NewGuid(),
            ShippingDetail = new ShippingDetailDTO { FullName = "X", PhoneNumber = "+1", City = "X", Address = "X" },
            Items = []
        };

        var result = await CreateService().CreateOrderAsync("user-1", request);

        Assert.False(result.Success);
        Assert.Equal("Order must contain at least one item.", result.Message);
        _orderRepository.Verify(x => x.SaveAsync(It.IsAny<Order>()), Times.Never);
    }

    // ── Guard: user not found ─────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userManagement.Setup(x => x.GetByIdAsync("user-1")).ReturnsAsync((AppUser?)null);

        var result = await CreateService().CreateOrderAsync("user-1", ValidRequest());

        Assert.False(result.Success);
        Assert.Equal("User not found.", result.Message);
        _orderRepository.Verify(x => x.SaveAsync(It.IsAny<Order>()), Times.Never);
    }

    // ── Guard: invalid payment method ─────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_ReturnsFailure_WhenPaymentMethodInvalid()
    {
        SetupValidUser();
        _paymentMethodService.Setup(x => x.GetMethodsAsync())
            .ReturnsAsync([new GetPaymentMethodDTO { Id = Guid.NewGuid(), Name = "Cash" }]);

        // request uses a different Guid that doesn't match
        var result = await CreateService().CreateOrderAsync("user-1", ValidRequest(Guid.NewGuid()));

        Assert.False(result.Success);
        Assert.Equal("Invalid payment method.", result.Message);
        _orderRepository.Verify(x => x.SaveAsync(It.IsAny<Order>()), Times.Never);
    }

    // ── Guard: variant not found ──────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_ReturnsFailure_WhenVariantNotFound()
    {
        var methodId = Guid.NewGuid();
        SetupValidUser();
        SetupPaymentMethod(methodId);
        _variantRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductVariant?)null);

        var result = await CreateService().CreateOrderAsync("user-1", ValidRequest(methodId));

        Assert.False(result.Success);
        Assert.Equal("Variant not found.", result.Message);
    }

    // ── Guard: inactive variant ───────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_ReturnsFailure_WhenVariantInactive()
    {
        var methodId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SetupValidUser();
        SetupPaymentMethod(methodId);
        _variantRepository.Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync(new ProductVariant { Id = variantId, IsActive = false, StockQuantity = 10, Price = 1000 });

        var request = ValidRequest(methodId);
        request.Items[0].VariantId = variantId;

        var result = await CreateService().CreateOrderAsync("user-1", request);

        Assert.False(result.Success);
        Assert.Equal("Variant is inactive.", result.Message);
    }

    // ── Guard: insufficient stock ─────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_ReturnsFailure_WhenInsufficientStock()
    {
        var methodId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SetupValidUser();
        SetupPaymentMethod(methodId);
        _variantRepository.Setup(x => x.GetByIdAsync(variantId))
            .ReturnsAsync(new ProductVariant { Id = variantId, IsActive = true, StockQuantity = 1, Price = 1000 });
        _variantRepository.Setup(x => x.DecrementStockAsync(variantId, It.IsAny<int>())).ReturnsAsync(false);

        var request = ValidRequest(methodId);
        request.Items[0] = new CreateOrderItemDTO { VariantId = variantId, Quantity = 5 };

        var result = await CreateService().CreateOrderAsync("user-1", request);

        Assert.False(result.Success);
        Assert.Equal("Insufficient stock.", result.Message);
    }

    // ── Happy path: order created ─────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_Succeeds_AndDeductsStock()
    {
        var methodId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SetupValidUser();
        SetupPaymentMethod(methodId);
        var variant = SetupVariant(variantId, price: 3000, stock: 10);

        var request = ValidRequest(methodId);
        request.Items[0] = new CreateOrderItemDTO { VariantId = variantId, Quantity = 3 };

        var result = await CreateService().CreateOrderAsync("user-1", request);

        Assert.True(result.Success);
        _variantRepository.Verify(x => x.DecrementStockAsync(variantId, 3), Times.Once);
        _orderRepository.Verify(x => x.SaveAsync(It.Is<Order>(o =>
            o.TotalAmount == 9000m &&
            o.Items.Count == 1 &&
            o.Status == OrderStatus.Pending)), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_PersistsShippingDetail_WithPostalCode()
    {
        var methodId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SetupValidUser();
        SetupPaymentMethod(methodId);
        SetupVariant(variantId);

        var request = ValidRequest(methodId);
        request.Items[0].VariantId = variantId;
        request.ShippingDetail.PostalCode = "0019";

        Order? saved = null;
        _orderRepository.Setup(x => x.SaveAsync(It.IsAny<Order>()))
            .Callback<Order>(o => saved = o);

        await CreateService().CreateOrderAsync("user-1", request);

        Assert.NotNull(saved);
        Assert.Equal("0019", saved!.ShippingDetail.PostalCode);
    }

    // ── Coupon applied ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_AppliesPercentageCoupon_AndDeductsDiscount()
    {
        var methodId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SetupValidUser();
        SetupPaymentMethod(methodId);
        SetupVariant(variantId, price: 10_000, stock: 5);

        var couponId = Guid.NewGuid();
        _couponRepository.Setup(x => x.GetByCodeAsync("SAVE10")).ReturnsAsync(new Domain.Entities.Coupons.Coupon
        {
            Id = couponId,
            Code = "SAVE10",
            Status = ActivityStatus.Active,
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10,
            CommissionRate = 5
        });

        var request = ValidRequest(methodId, couponCode: "SAVE10");
        request.Items[0] = new CreateOrderItemDTO { VariantId = variantId, Quantity = 1 };

        Order? saved = null;
        _orderRepository.Setup(x => x.SaveAsync(It.IsAny<Order>())).Callback<Order>(o => saved = o);

        var result = await CreateService().CreateOrderAsync("user-1", request);

        Assert.True(result.Success);
        Assert.NotNull(saved);
        Assert.Equal(9_000m, saved!.TotalAmount);   // 10000 - 10%
        _couponRepository.Verify(x => x.UpdateAsync(It.Is<Domain.Entities.Coupons.Coupon>(c => c.UsedCount == 1)), Times.Once);
        _couponRepository.Verify(x => x.AddCouponOrderAsync(It.IsAny<Domain.Entities.Orders.CouponOrder>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_SkipsCoupon_WhenCodeIsBlank()
    {
        var methodId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SetupValidUser();
        SetupPaymentMethod(methodId);
        SetupVariant(variantId, price: 5000);

        var request = ValidRequest(methodId, couponCode: null);
        request.Items[0].VariantId = variantId;

        await CreateService().CreateOrderAsync("user-1", request);

        _couponRepository.Verify(x => x.GetByCodeAsync(It.IsAny<string>()), Times.Never);
        _couponRepository.Verify(x => x.AddCouponOrderAsync(It.IsAny<Domain.Entities.Orders.CouponOrder>()), Times.Never);
    }

    [Fact]
    public async Task CreateOrderAsync_SkipsCoupon_WhenCouponNotFound()
    {
        var methodId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        SetupValidUser();
        SetupPaymentMethod(methodId);
        SetupVariant(variantId, price: 5000);

        _couponRepository.Setup(x => x.GetByCodeAsync("GHOST")).ReturnsAsync((Domain.Entities.Coupons.Coupon?)null);

        var request = ValidRequest(methodId, couponCode: "GHOST");
        request.Items[0].VariantId = variantId;

        Order? saved = null;
        _orderRepository.Setup(x => x.SaveAsync(It.IsAny<Order>())).Callback<Order>(o => saved = o);

        var result = await CreateService().CreateOrderAsync("user-1", request);

        Assert.True(result.Success);
        Assert.Equal(5000m, saved!.TotalAmount);  // no discount applied
        _couponRepository.Verify(x => x.AddCouponOrderAsync(It.IsAny<Domain.Entities.Orders.CouponOrder>()), Times.Never);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_UsesSafeDefaults_WhenShippingDetailMissing()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(new Order
        {
            Id = orderId,
            UserId = "user-1",
            CustomerName = "John",
            CustomerEmail = "john@example.com",
            ShippingDetail = null!,
            Items = []
        });

        var result = await CreateService().GetByIdAsync(orderId);

        Assert.NotNull(result);
        Assert.NotNull(result!.ShippingDetail);
        Assert.Equal(string.Empty, result.ShippingDetail.FullName);
        Assert.Equal(string.Empty, result.ShippingDetail.PhoneNumber);
        Assert.Equal(string.Empty, result.ShippingDetail.City);
        Assert.Equal(string.Empty, result.ShippingDetail.Address);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenOrderNotFound()
    {
        _orderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var result = await CreateService().GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── UpdateStatusAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFailure_WhenOrderNotFound()
    {
        _orderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var result = await CreateService().UpdateStatusAsync(new UpdateOrderStatusDTO
        {
            OrderId = Guid.NewGuid(),
            Status  = OrderStatus.Paid
        });

        Assert.False(result.Success);
        Assert.Equal("Order not found", result.Message);
        _orderRepository.Verify(x => x.UpdateStatus(It.IsAny<Guid>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsUnchanged_WhenStatusIsSame()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(new Order
        {
            Id = orderId, Status = OrderStatus.Pending, Items = []
        });

        var result = await CreateService().UpdateStatusAsync(new UpdateOrderStatusDTO
        {
            OrderId = orderId,
            Status  = OrderStatus.Pending
        });

        Assert.True(result.Success);
        Assert.Equal("Status unchanged", result.Message);
        _orderRepository.Verify(x => x.UpdateStatus(It.IsAny<Guid>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFailure_WhenTransitionInvalid()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(new Order
        {
            Id = orderId, Status = OrderStatus.Completed, Items = []
        });

        var result = await CreateService().UpdateStatusAsync(new UpdateOrderStatusDTO
        {
            OrderId = orderId,
            Status  = OrderStatus.Pending  // Completed → Pending is invalid
        });

        Assert.False(result.Success);
        Assert.Contains("Cannot change order status", result.Message);
        _orderRepository.Verify(x => x.UpdateStatus(It.IsAny<Guid>(), It.IsAny<OrderStatus>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_Succeeds_WhenTransitionIsValid()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(new Order
        {
            Id = orderId, Status = OrderStatus.Pending, Items = []
        });
        _orderRepository.Setup(x => x.UpdateStatus(orderId, OrderStatus.Paid)).ReturnsAsync(1);

        var result = await CreateService().UpdateStatusAsync(new UpdateOrderStatusDTO
        {
            OrderId = orderId,
            Status  = OrderStatus.Paid
        });

        Assert.True(result.Success);
        Assert.Equal("Updated successfully", result.Message);
        _orderRepository.Verify(x => x.UpdateStatus(orderId, OrderStatus.Paid), Times.Once);
    }

    // ── CheckoutAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckoutAsync_ReturnsFailure_WhenOrderNotFound()
    {
        _orderRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Order?)null);

        var result = await CreateService().CheckoutAsync(new CheckoutDTO
        {
            OrderId = Guid.NewGuid(), PaymentMethodId = Guid.NewGuid()
        });

        Assert.False(result.Success);
        Assert.Equal("Order not found.", result.Message);
    }

    [Fact]
    public async Task CheckoutAsync_ReturnsFailure_WhenOrderStatusNotPending()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(new Order
        {
            Id = orderId, Status = OrderStatus.Paid, Items = []
        });

        var result = await CreateService().CheckoutAsync(new CheckoutDTO
        {
            OrderId = orderId, PaymentMethodId = Guid.NewGuid()
        });

        Assert.False(result.Success);
        Assert.Equal("Invalid order status.", result.Message);
    }

    [Fact]
    public async Task CheckoutAsync_ReturnsFailure_WhenPaymentMethodInvalid()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(new Order
        {
            Id = orderId, Status = OrderStatus.Pending, Items = []
        });
        _paymentMethodService.Setup(x => x.GetMethodsAsync())
            .ReturnsAsync([new GetPaymentMethodDTO { Id = Guid.NewGuid(), Name = "Cash" }]);

        var result = await CreateService().CheckoutAsync(new CheckoutDTO
        {
            OrderId = orderId, PaymentMethodId = Guid.NewGuid()  // not in list
        });

        Assert.False(result.Success);
        Assert.Equal("Invalid payment method.", result.Message);
        _orderRepository.Verify(x => x.UpdateCheckoutAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task CheckoutAsync_Succeeds_AndCallsUpdateCheckout()
    {
        var orderId   = Guid.NewGuid();
        var methodId  = Guid.NewGuid();

        _orderRepository.Setup(x => x.GetByIdAsync(orderId)).ReturnsAsync(new Order
        {
            Id = orderId, Status = OrderStatus.Pending, Items = []
        });
        _paymentMethodService.Setup(x => x.GetMethodsAsync())
            .ReturnsAsync([new GetPaymentMethodDTO { Id = methodId, Name = "Cash" }]);
        _orderRepository.Setup(x => x.UpdateCheckoutAsync(orderId)).ReturnsAsync(1);

        var result = await CreateService().CheckoutAsync(new CheckoutDTO
        {
            OrderId = orderId, PaymentMethodId = methodId
        });

        Assert.True(result.Success);
        Assert.Equal("Payment successful.", result.Message);
        _orderRepository.Verify(x => x.UpdateCheckoutAsync(orderId), Times.Once);
    }
}
