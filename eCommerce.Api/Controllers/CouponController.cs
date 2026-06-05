using eCommerce.Application.DTOs.Coupons;
using eCommerce.Application.Services.Interfaces.Coupons;
using eCommerce.Application.Services.Interfaces.Influencers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponController(ICouponService couponService, IInfluencerService influencerService) : ControllerBase
    {
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await couponService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("by-influencer/{influencerId:guid}")]
        [Authorize(Roles = "Admin,Influencer")]
        public async Task<IActionResult> GetByInfluencer(Guid influencerId)
        {
            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var caller = await influencerService.GetByUserIdAsync(userId!);
                if (caller?.Id != influencerId)
                    return Forbid();
            }

            var result = await couponService.GetByInfluencerAsync(influencerId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCouponDTO dto)
        {
            var result = await couponService.CreateAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateCouponDTO dto)
        {
            var result = await couponService.UpdateAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateCouponStatusDTO dto)
        {
            var result = await couponService.UpdateStatusAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("validate")]
        [Authorize]
        [EnableRateLimiting("api")]
        public async Task<IActionResult> Validate([FromBody] ValidateCouponDTO dto)
        {
            var result = await couponService.ValidateAsync(dto);
            return result.IsValid ? Ok(result) : BadRequest(result);
        }
    }
}
