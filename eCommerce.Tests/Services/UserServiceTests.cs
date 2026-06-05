using eCommerce.Application.DTOs.Addresses;
using eCommerce.Application.DTOs.Users;
using eCommerce.Application.Services.Implementations.Users;
using eCommerce.Domain.Entities.Addresses;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Influencers;
using eCommerce.Domain.Interfaces.Users;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace eCommerce.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserManagement> _userManagement = new();
    private readonly Mock<IInfluencerRepository> _influencerRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();

    private UserService CreateService() => new(_userManagement.Object, _influencerRepo.Object, _userRepo.Object);

    // ── CreateAddressAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAddressAsync_UndefaultsExistingAddresses_WhenNewIsDefault()
    {
        var existing = new List<UserAddress>
        {
            new() { Id = Guid.NewGuid(), UserId = "u1", IsDefault = true },
            new() { Id = Guid.NewGuid(), UserId = "u1", IsDefault = false }
        };
        _userRepo.Setup(x => x.GetUserAddressesAsync("u1")).ReturnsAsync(existing);

        var dto = new CreateAddressDTO
        {
            FullName = "Jane", PhoneNumber = "+1", City = "LA",
            AddressLine = "Main St", IsDefault = true
        };

        await CreateService().CreateAddressAsync("u1", dto);

        Assert.All(existing, a => Assert.False(a.IsDefault));
        _userRepo.Verify(x => x.UpdateAsync(It.IsAny<UserAddress>()), Times.Exactly(existing.Count));
    }

    [Fact]
    public async Task CreateAddressAsync_DoesNotTouchExistingAddresses_WhenNewIsNotDefault()
    {
        _userRepo.Setup(x => x.GetUserAddressesAsync(It.IsAny<string>())).ReturnsAsync([]);

        var dto = new CreateAddressDTO
        {
            FullName = "Jane", PhoneNumber = "+1", City = "LA",
            AddressLine = "Main St", IsDefault = false
        };

        await CreateService().CreateAddressAsync("u1", dto);

        _userRepo.Verify(x => x.GetUserAddressesAsync(It.IsAny<string>()), Times.Never);
        _userRepo.Verify(x => x.UpdateAsync(It.IsAny<UserAddress>()), Times.Never);
    }

    [Fact]
    public async Task CreateAddressAsync_PersistsAllFields_IncludingPostalCode()
    {
        _userRepo.Setup(x => x.GetUserAddressesAsync(It.IsAny<string>())).ReturnsAsync([]);

        UserAddress? saved = null;
        _userRepo.Setup(x => x.CreateAddressAsync(It.IsAny<UserAddress>()))
            .Callback<UserAddress>(a => saved = a);

        var dto = new CreateAddressDTO
        {
            FullName = "Alice",
            PhoneNumber = "+37400000000",
            City = "Yerevan",
            AddressLine = "Baghramyan 1",
            PostalCode = "0019",
            Notes = "Ring twice",
            IsDefault = false
        };

        var result = await CreateService().CreateAddressAsync("u1", dto);

        Assert.True(result.Success);
        Assert.NotNull(saved);
        Assert.Equal("Alice", saved!.FullName);
        Assert.Equal("0019", saved.PostalCode);
        Assert.Equal("Ring twice", saved.Notes);
        _userRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    // ── UpdateAddressAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAddressAsync_ReturnsFailure_WhenAddressNotFound()
    {
        _userRepo.Setup(x => x.GetUserAddressesAsync("u1")).ReturnsAsync([]);

        var result = await CreateService().UpdateAddressAsync("u1", new UpdateAddressDTO
        {
            Id = Guid.NewGuid(), FullName = "X", PhoneNumber = "+1", City = "X", AddressLine = "X"
        });

        Assert.False(result.Success);
        Assert.Equal("Address not found", result.Message);
    }

    [Fact]
    public async Task UpdateAddressAsync_UndefaultsOtherAddresses_WhenBecomingDefault()
    {
        var targetId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        var addresses = new List<UserAddress>
        {
            new() { Id = targetId, UserId = "u1", IsDefault = false },
            new() { Id = otherId,  UserId = "u1", IsDefault = true }
        };

        _userRepo.Setup(x => x.GetUserAddressesAsync("u1")).ReturnsAsync(addresses);

        await CreateService().UpdateAddressAsync("u1", new UpdateAddressDTO
        {
            Id = targetId, FullName = "A", PhoneNumber = "+1", City = "X", AddressLine = "X",
            IsDefault = true
        });

        var other = addresses.First(a => a.Id == otherId);
        Assert.False(other.IsDefault);
    }

    [Fact]
    public async Task UpdateAddressAsync_UpdatesAllFields_IncludingPostalCode()
    {
        var id = Guid.NewGuid();
        var address = new UserAddress { Id = id, UserId = "u1", FullName = "Old", PostalCode = "0001" };
        _userRepo.Setup(x => x.GetUserAddressesAsync("u1")).ReturnsAsync([address]);

        var result = await CreateService().UpdateAddressAsync("u1", new UpdateAddressDTO
        {
            Id = id,
            FullName = "New",
            PhoneNumber = "+37400000001",
            City = "Gyumri",
            AddressLine = "Shahumyan 5",
            PostalCode = "3100",
            Notes = "Leave at door",
            IsDefault = false
        });

        Assert.True(result.Success);
        Assert.Equal("New", address.FullName);
        Assert.Equal("3100", address.PostalCode);
        Assert.Equal("Gyumri", address.City);
        Assert.Equal("Leave at door", address.Notes);
        _userRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    // ── DeleteAddressAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAddressAsync_ReturnsFailure_WhenNotFound()
    {
        _userRepo.Setup(x => x.GetUserAddressAsync("u1", It.IsAny<Guid>())).ReturnsAsync((UserAddress?)null);

        var result = await CreateService().DeleteAddressAsync("u1", Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("Not existing address", result.Message);
        _userRepo.Verify(x => x.DeleteAsync(It.IsAny<UserAddress>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAddressAsync_DeletesAndSaves_WhenFound()
    {
        var address = new UserAddress { Id = Guid.NewGuid(), UserId = "u1" };
        _userRepo.Setup(x => x.GetUserAddressAsync("u1", address.Id)).ReturnsAsync(address);

        var result = await CreateService().DeleteAddressAsync("u1", address.Id);

        Assert.True(result.Success);
        _userRepo.Verify(x => x.DeleteAsync(address), Times.Once);
        _userRepo.Verify(x => x.SaveAsync(), Times.Once);
    }

    // ── GetUserAddressesAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetUserAddressesAsync_MapsAllFields_IncludingPostalCode()
    {
        _userRepo.Setup(x => x.GetUserAddressesAsync("u1")).ReturnsAsync(
        [
            new UserAddress
            {
                Id = Guid.NewGuid(),
                FullName = "Bob",
                PhoneNumber = "+1",
                City = "Berlin",
                AddressLine = "Karl-Marx 1",
                PostalCode = "10115",
                Notes = "Doorbell broken",
                IsDefault = true
            }
        ]);

        var result = await CreateService().GetUserAddressesAsync("u1");

        var dto = Assert.Single(result);
        Assert.Equal("Bob", dto.FullName);
        Assert.Equal("10115", dto.PostalCode);
        Assert.Equal("Doorbell broken", dto.Notes);
        Assert.True(dto.IsDefault);
    }

    // ── UpdateProfileAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfileAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userManagement.Setup(x => x.GetByIdAsync("u1")).ReturnsAsync((AppUser?)null);

        var result = await CreateService().UpdateProfileAsync(new UserProfileDTO { Id = "u1" });

        Assert.False(result.Success);
        Assert.Equal("User not found", result.Message);
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdatesUserAndInfluencer_WhenSucceeds()
    {
        var user = new AppUser { Id = "u1", FullName = "Old", Email = "old@x.com", PhoneNumber = "+0" };
        _userManagement.Setup(x => x.GetByIdAsync("u1")).ReturnsAsync(user);
        _userManagement.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _influencerRepo.Setup(x => x.GetByIdAsync("u1")).ReturnsAsync((eCommerce.Domain.Entities.Influencers.Influencer?)null);

        var result = await CreateService().UpdateProfileAsync(new UserProfileDTO
        {
            Id = "u1",
            Name = "New Name",
            Email = "new@x.com",
            Phone = "+37400000000"
        });

        Assert.True(result.Success);
        Assert.Equal("New Name", user.FullName);
        Assert.Equal("new@x.com", user.Email);
        Assert.Equal("+37400000000", user.PhoneNumber);
    }

    // ── GetProfileAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetProfileAsync_ReturnsCorrectProfile()
    {
        _userManagement.Setup(x => x.GetByIdAsync("u1")).ReturnsAsync(new AppUser
        {
            Id = "u1",
            FullName = "Maria",
            Email = "maria@x.com",
            PhoneNumber = "+37499999999"
        });

        var result = await CreateService().GetProfileAsync("u1");

        Assert.Equal("u1", result.Id);
        Assert.Equal("Maria", result.Name);
        Assert.Equal("maria@x.com", result.Email);
        Assert.Equal("+37499999999", result.Phone);
    }
}
