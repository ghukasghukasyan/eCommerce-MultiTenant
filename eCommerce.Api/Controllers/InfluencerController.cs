using eCommerce.Application.DTOs.Influencers;
using eCommerce.Application.Services.Interfaces.Influencers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class InfluencerController(IInfluencerService service) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] CreateInfluencerDTO dto)
        {
            var result = await service.CreateAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Influencer")]
        public async Task<IActionResult> GetMeAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await service.GetByUserIdAsync(userId!);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Influencer")]
        public async Task<IActionResult> GetMyStatsAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await service.GetMyStatsAsync(userId!);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInfluencerStatusAsync([FromBody] UpdateInfluencerStatusDTO request)
        {
            var result = await service.UpdateInfluencerStatusAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("byId/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await service.GetByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpGet("{id}/stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStatsByIdAsync(Guid id)
        {
            var result = await service.GetStatsByInfluencerIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost("{id}/payout")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecordPayoutAsync(Guid id, [FromBody] RecordPayoutRequest request)
        {
            var result = await service.RecordPayoutAsync(new()
            {
                InfluencerId = id,
                Amount = request.Amount,
                Note = request.Note
            });
            return result.Success ? Ok(result) : BadRequest(result);
        }

        public record RecordPayoutRequest(decimal Amount, string? Note);

        [HttpPost("uploadAvatar/{influencerId}")]
        [Authorize(Roles = "Admin,Influencer")]
        public async Task<IActionResult> UploadAvatarAsync(Guid influencerId,
            IFormFile file)
        {
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var influencer = await service.GetByUserIdAsync(userId!);
                if (influencer?.Id != influencerId)
                    return Forbid();
            }

            await service.UploadAvatarAsync(influencerId, file);
            return NoContent();
        }
    }
}
