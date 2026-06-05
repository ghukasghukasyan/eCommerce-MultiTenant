using AutoMapper;
using eCommerce.Application.Constants;
using eCommerce.Application.DTOs.Identity;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Implementations.Authentication;
using eCommerce.Application.Services.Interfaces;
using eCommerce.Application.Services.Interfaces.Logging;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Entities.Influencers;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Influencers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Claims;

namespace eCommerce.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<ITokenManagement>    _token        = new();
    private readonly Mock<IUserManagement>     _userMgmt     = new();
    private readonly Mock<IRoleManagement>     _roleMgmt     = new();
    private readonly Mock<IAppLogger<AuthenticationService>> _logger = new();
    private readonly Mock<IMapper>             _mapper       = new();
    private readonly Mock<IInfluencerRepository> _influencerRepo = new();
    private readonly Mock<IEmailService>       _emailService = new();
    private readonly Mock<IConfiguration>      _config       = new();

    private AuthenticationService CreateService() => new(
        _token.Object, _userMgmt.Object, _roleMgmt.Object,
        _logger.Object, _mapper.Object, _influencerRepo.Object,
        _emailService.Object, _config.Object);

    private static AppUser MakeUser(string email = "user@test.com") =>
        new() { Id = Guid.NewGuid().ToString(), Email = email, FullName = "Test User" };

    private static IdentityResult Ok() => IdentityResult.Success;
    private static IdentityResult Fail(string code, string desc) =>
        IdentityResult.Failed(new IdentityError { Code = code, Description = desc });

    // ── RegisterAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task RegisterAsync_ReturnsFailure_WhenRegistrationFails()
    {
        var dto = new RegisterUserDTO { Email = "a@b.com", Password = "P@ss1", FullName = "A", PhoneNumber = "1" };
        _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(MakeUser(dto.Email));
        _userMgmt.Setup(u => u.RegisterAsync(It.IsAny<AppUser>(), dto.Password))
                 .ReturnsAsync(Fail("DuplicateEmail", "Email already taken."));

        var result = await CreateService().RegisterAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Email already taken.", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsSuccess_AndSendsConfirmationEmail()
    {
        var dto  = new RegisterUserDTO { Email = "new@b.com", Password = "P@ss1", FullName = "New", PhoneNumber = "1" };
        var user = MakeUser(dto.Email);

        _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(user);
        _userMgmt.Setup(u => u.RegisterAsync(It.IsAny<AppUser>(), dto.Password)).ReturnsAsync(Ok());
        _userMgmt.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.AnyOtherUserExistsAsync(user.Id)).ReturnsAsync(true); // other users exist → "User" role
        _roleMgmt.Setup(r => r.AddUserToRole(user, "User")).ReturnsAsync(true);
        _userMgmt.Setup(u => u.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("tok");
        _config.Setup(c => c["Frontend:BaseUrl"]).Returns("http://localhost");
        _emailService.Setup(e => e.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

        var result = await CreateService().RegisterAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("Account Created", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_AssignsAdminRole_WhenFirstUser()
    {
        var dto  = new RegisterUserDTO { Email = "first@b.com", Password = "P@ss1", FullName = "Admin", PhoneNumber = "1" };
        var user = MakeUser(dto.Email);

        _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(user);
        _userMgmt.Setup(u => u.RegisterAsync(It.IsAny<AppUser>(), dto.Password)).ReturnsAsync(Ok());
        _userMgmt.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.AnyOtherUserExistsAsync(user.Id)).ReturnsAsync(false); // no other users → "Admin" role
        _roleMgmt.Setup(r => r.AddUserToRole(user, "Admin")).ReturnsAsync(true);
        _userMgmt.Setup(u => u.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("tok");
        _config.Setup(c => c["Frontend:BaseUrl"]).Returns("http://localhost");
        _emailService.Setup(e => e.SendEmailConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

        await CreateService().RegisterAsync(dto);

        _roleMgmt.Verify(r => r.AddUserToRole(user, "Admin"), Times.Once);
        _roleMgmt.Verify(r => r.AddUserToRole(user, "User"), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_RollsBackUser_WhenRoleAssignmentFails()
    {
        var dto  = new RegisterUserDTO { Email = "x@b.com", Password = "P@ss1", FullName = "X", PhoneNumber = "1" };
        var user = MakeUser(dto.Email);

        _mapper.Setup(m => m.Map<AppUser>(dto)).Returns(user);
        _userMgmt.Setup(u => u.RegisterAsync(It.IsAny<AppUser>(), dto.Password)).ReturnsAsync(Ok());
        _userMgmt.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.AnyOtherUserExistsAsync(user.Id)).ReturnsAsync(true);
        _roleMgmt.Setup(r => r.AddUserToRole(user, "User")).ReturnsAsync(false);
        _userMgmt.Setup(u => u.RemoveByEmailAsync(user.Email!)).ReturnsAsync(1);

        var result = await CreateService().RegisterAsync(dto);

        Assert.False(result.Success);
        _userMgmt.Verify(u => u.RemoveByEmailAsync(user.Email!), Times.Once);
    }

    // ── RegisterInfluencerAsync ─────────────────────────────────────────────

    [Fact]
    public async Task RegisterInfluencerAsync_ReturnsFailure_WhenEmailAlreadyExists()
    {
        var dto = new RegisterInfluencerDTO
        {
            Email = "exists@test.com", Password = "P@ss1",
            FullName = "Jane", PhoneNumber = "1",
            InstagramAccountUrl = "ig", TikTokAccountUrl = "tt"
        };

        _userMgmt.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(MakeUser(dto.Email));

        var result = await CreateService().RegisterInfluencerAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("User already exists.", result.Message);
        _influencerRepo.Verify(r => r.AddAsync(It.IsAny<Influencer>()), Times.Never);
    }

    [Fact]
    public async Task RegisterInfluencerAsync_ReturnsFailure_WhenRegistrationFails()
    {
        var dto = new RegisterInfluencerDTO
        {
            Email = "new@test.com", Password = "weak",
            FullName = "Jane", PhoneNumber = "1",
            InstagramAccountUrl = "ig", TikTokAccountUrl = "tt"
        };

        _userMgmt.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);
        _userMgmt.Setup(u => u.RegisterAsync(It.IsAny<AppUser>(), dto.Password))
                 .ReturnsAsync(Fail("PasswordTooWeak", "Password too weak."));

        var result = await CreateService().RegisterInfluencerAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Password too weak.", result.Message);
    }

    [Fact]
    public async Task RegisterInfluencerAsync_CreatesInfluencerProfile_WhenSuccessful()
    {
        var dto = new RegisterInfluencerDTO
        {
            Email = "inf@test.com", Password = "P@ss1!",
            FullName = "Jane", PhoneNumber = "+37400000000",
            InstagramAccountUrl = "https://ig.com/jane",
            TikTokAccountUrl    = "https://tiktok.com/jane"
        };
        var user = MakeUser(dto.Email);

        _userMgmt.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync((AppUser?)null);
        _userMgmt.Setup(u => u.RegisterAsync(It.IsAny<AppUser>(), dto.Password)).ReturnsAsync(Ok());
        _roleMgmt.Setup(r => r.AddUserToRole(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(true);

        Influencer? saved = null;
        _influencerRepo.Setup(r => r.AddAsync(It.IsAny<Influencer>()))
                       .Callback<Influencer>(i => saved = i);

        var result = await CreateService().RegisterInfluencerAsync(dto);

        Assert.True(result.Success);
        Assert.NotNull(saved);
        Assert.Equal("Jane", saved!.FullName);
        Assert.Equal("inf@test.com", saved.Email);
        Assert.Equal(InfluencerConstants.DefaultCommissionRate, saved.DefaultCommissionRate);
        _influencerRepo.Verify(r => r.AddAsync(It.IsAny<Influencer>()), Times.Once);
    }

    // ── LoginUserAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task LoginUserAsync_ReturnsFailure_WhenCredentialsInvalid()
    {
        var dto = new LoginUserDTO { Email = "x@b.com", Password = "wrong" };
        _userMgmt.Setup(u => u.LoginAsync(dto.Email, dto.Password)).ReturnsAsync(false);

        var result = await CreateService().LoginUserAsync(dto);

        Assert.False(result.Success);
        Assert.Equal("Invalid credentials", result.Message);
    }

    [Fact]
    public async Task LoginUserAsync_ReturnsTokens_WhenCredentialsValid()
    {
        var dto  = new LoginUserDTO { Email = "user@b.com", Password = "P@ss1" };
        var user = MakeUser(dto.Email);
        var claims = new List<Claim> { new(ClaimTypes.Email, dto.Email) };

        _userMgmt.Setup(u => u.LoginAsync(dto.Email, dto.Password)).ReturnsAsync(true);
        _userMgmt.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.GetClaimsAsync(user.Email!)).ReturnsAsync(claims);
        _token.Setup(t => t.GenerateToken(claims)).Returns("jwt-token");
        _token.Setup(t => t.GetRefreshToken()).Returns("refresh-token");
        _token.Setup(t => t.ValidateRefreshToken("refresh-token")).ReturnsAsync(false);
        _token.Setup(t => t.AddRefreshToken(user.Id, "refresh-token")).ReturnsAsync(1);

        var result = await CreateService().LoginUserAsync(dto);

        Assert.True(result.Success);
        Assert.Equal("jwt-token", result.Token);
        Assert.Equal("refresh-token", result.RefreshToken);
    }

    [Fact]
    public async Task LoginUserAsync_ReturnsFailure_WhenTokenSaveFails()
    {
        var dto  = new LoginUserDTO { Email = "user@b.com", Password = "P@ss1" };
        var user = MakeUser(dto.Email);
        var claims = new List<Claim>();

        _userMgmt.Setup(u => u.LoginAsync(dto.Email, dto.Password)).ReturnsAsync(true);
        _userMgmt.Setup(u => u.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.GetClaimsAsync(user.Email!)).ReturnsAsync(claims);
        _token.Setup(t => t.GenerateToken(claims)).Returns("jwt");
        _token.Setup(t => t.GetRefreshToken()).Returns("rt");
        _token.Setup(t => t.ValidateRefreshToken("rt")).ReturnsAsync(false);
        _token.Setup(t => t.AddRefreshToken(user.Id, "rt")).ReturnsAsync(0); // fail

        var result = await CreateService().LoginUserAsync(dto);

        Assert.False(result.Success);
    }

    // ── ReviveTokenAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ReviveTokenAsync_ReturnsFailure_WhenTokenInvalid()
    {
        _token.Setup(t => t.ValidateRefreshToken("bad-token")).ReturnsAsync(false);

        var result = await CreateService().ReviveTokenAsync("bad-token");

        Assert.False(result.Success);
        Assert.Equal("Invalid token", result.Message);
    }

    [Fact]
    public async Task ReviveTokenAsync_ReturnsFailure_WhenUserIdNotFound()
    {
        _token.Setup(t => t.ValidateRefreshToken("rt")).ReturnsAsync(true);
        _token.Setup(t => t.GetUserIdByRefreshToken("rt")).ReturnsAsync(string.Empty);

        var result = await CreateService().ReviveTokenAsync("rt");

        Assert.False(result.Success);
        Assert.Equal("Invalid token", result.Message);
    }

    [Fact]
    public async Task ReviveTokenAsync_ReturnsNewTokens_WhenValid()
    {
        var user   = MakeUser();
        var claims = new List<Claim> { new(ClaimTypes.Email, user.Email!) };

        _token.Setup(t => t.ValidateRefreshToken("old-rt")).ReturnsAsync(true);
        _token.Setup(t => t.GetUserIdByRefreshToken("old-rt")).ReturnsAsync(user.Id);
        _userMgmt.Setup(u => u.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.GetClaimsAsync(user.Email!)).ReturnsAsync(claims);
        _token.Setup(t => t.GenerateToken(claims)).Returns("new-jwt");
        _token.Setup(t => t.GetRefreshToken()).Returns("new-rt");
        _token.Setup(t => t.UpdateRefreshToken(user.Id, "new-rt")).ReturnsAsync(1);

        var result = await CreateService().ReviveTokenAsync("old-rt");

        Assert.True(result.Success);
        Assert.Equal("new-jwt", result.Token);
        Assert.Equal("new-rt", result.RefreshToken);
    }

    // ── ConfirmEmailAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ConfirmEmailAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userMgmt.Setup(u => u.GetByIdAsync("uid")).ReturnsAsync((AppUser?)null);

        var result = await CreateService().ConfirmEmailAsync("uid", "tok");

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ReturnsSuccess_WhenTokenValid()
    {
        var user = MakeUser();
        _userMgmt.Setup(u => u.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.ConfirmEmailAsync(user, "tok")).ReturnsAsync(Ok());

        var result = await CreateService().ConfirmEmailAsync(user.Id, "tok");

        Assert.True(result.Success);
        Assert.Equal("Email confirmed successfully", result.Message);
    }

    [Fact]
    public async Task ConfirmEmailAsync_ReturnsFailure_WhenTokenInvalid()
    {
        var user = MakeUser();
        _userMgmt.Setup(u => u.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.ConfirmEmailAsync(user, "bad"))
                 .ReturnsAsync(Fail("InvalidToken", "Invalid token."));

        var result = await CreateService().ConfirmEmailAsync(user.Id, "bad");

        Assert.False(result.Success);
        Assert.Equal("Invalid token.", result.Message);
    }

    // ── ForgotPasswordAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task ForgotPasswordAsync_ReturnsSuccess_EvenWhenUserNotFound()
    {
        _userMgmt.Setup(u => u.GetByEmailAsync("ghost@x.com")).ReturnsAsync((AppUser?)null);

        var result = await CreateService().ForgotPasswordAsync("ghost@x.com");

        Assert.True(result.Success);
        _emailService.Verify(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPasswordAsync_SendsResetEmail_WhenUserExists()
    {
        var user = MakeUser("real@x.com");
        _userMgmt.Setup(u => u.GetByEmailAsync(user.Email!)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.GeneratePasswordResetTokenAsync(user)).ReturnsAsync("reset-tok");
        _config.Setup(c => c["Frontend:BaseUrl"]).Returns("http://localhost");
        _emailService.Setup(e => e.SendPasswordResetAsync(user.Email!, user.FullName, It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

        var result = await CreateService().ForgotPasswordAsync(user.Email!);

        Assert.True(result.Success);
        _emailService.Verify(e => e.SendPasswordResetAsync(user.Email!, user.FullName, It.IsAny<string>()), Times.Once);
    }

    // ── ResetPasswordAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ResetPasswordAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userMgmt.Setup(u => u.GetByIdAsync("uid")).ReturnsAsync((AppUser?)null);

        var result = await CreateService().ResetPasswordAsync(new() { UserId = "uid", Token = "t", NewPassword = "P@ss1!" });

        Assert.False(result.Success);
        Assert.Equal("Invalid request", result.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsFailure_WhenResetFails()
    {
        var user = MakeUser();
        _userMgmt.Setup(u => u.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.ResetPasswordAsync(user, "bad-tok", "NewP@ss1!"))
                 .ReturnsAsync(Fail("InvalidToken", "Token expired."));

        var result = await CreateService().ResetPasswordAsync(new() { UserId = user.Id, Token = "bad-tok", NewPassword = "NewP@ss1!" });

        Assert.False(result.Success);
        Assert.Equal("Token expired.", result.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_ConfirmsEmail_WhenNotYetConfirmed()
    {
        var user = MakeUser();
        user.EmailConfirmed = false;
        _userMgmt.Setup(u => u.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.ResetPasswordAsync(user, "tok", "NewP@ss!")).ReturnsAsync(Ok());
        _userMgmt.Setup(u => u.UpdateAsync(user)).ReturnsAsync(Ok());

        var result = await CreateService().ResetPasswordAsync(new() { UserId = user.Id, Token = "tok", NewPassword = "NewP@ss!" });

        Assert.True(result.Success);
        Assert.True(user.EmailConfirmed);
        _userMgmt.Verify(u => u.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_DoesNotUpdateUser_WhenEmailAlreadyConfirmed()
    {
        var user = MakeUser();
        user.EmailConfirmed = true;
        _userMgmt.Setup(u => u.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _userMgmt.Setup(u => u.ResetPasswordAsync(user, "tok", "NewP@ss!")).ReturnsAsync(Ok());

        await CreateService().ResetPasswordAsync(new() { UserId = user.Id, Token = "tok", NewPassword = "NewP@ss!" });

        _userMgmt.Verify(u => u.UpdateAsync(It.IsAny<AppUser>()), Times.Never);
    }
}
