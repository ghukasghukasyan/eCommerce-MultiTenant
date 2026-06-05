using eCommerce.Application.DTOs.Addresses;
using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Users;
using eCommerce.Application.Services.Interfaces.Orders;
using eCommerce.Application.Services.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserOrderService orderService, IUserService userService) : ControllerBase
    {
        [HttpGet("orders")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> GetAllAsync([FromQuery] OrderFilterDTO filter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await orderService.GetAllAsync(filter, userId);
            return Ok(orders);
        }

        [HttpGet("order/{id}")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await orderService.GetByIdAsync(id, userId);
            return orders is null ? NotFound() : Ok(orders);
        }

        [HttpGet("profile")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await userService.GetProfileAsync(userId);
            return Ok(profile);
        }


        [HttpPut("profile")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> UpdateProfile(UserProfileDTO request)
        {
            request.Id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await userService.UpdateProfileAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("address")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> Create(CreateAddressDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await userService.CreateAddressAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("address")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> Update(UpdateAddressDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await userService.UpdateAddressAsync(userId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("address/{id}")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await userService.DeleteAddressAsync(userId, id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("address")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Ok(await userService.GetUserAddressesAsync(userId));
        }

    }
}
