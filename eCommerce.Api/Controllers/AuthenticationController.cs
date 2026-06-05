using eCommerce.Application.DTOs.Identity;
using eCommerce.Application.Services.Interfaces.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using System.Web;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("auth")]
    public class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterUserDTO userDTO)
        {
            var result = await authenticationService.RegisterAsync(userDTO);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("register/influencer")]
        public async Task<IActionResult> RegisterInfluencerAsync(RegisterInfluencerDTO influencerDTO)
        {
            var result = await authenticationService.RegisterInfluencerAsync(influencerDTO);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync(LoginUserDTO loginDTO)
        {
            var result = await authenticationService.LoginUserAsync(loginDTO);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("refreshToken/{refreshToken}")]
        public async Task<IActionResult> ReceiveTokenAsync(string refreshToken)
        {
            var result = await authenticationService.ReviveTokenAsync(HttpUtility.UrlDecode(refreshToken));
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await authenticationService.ConfirmEmailAsync(userId, HttpUtility.UrlDecode(token));
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordDTO dto)
        {
            var result = await authenticationService.ForgotPasswordAsync(dto.Email);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDTO dto)
        {
            var result = await authenticationService.ResetPasswordAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> LogoutAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await authenticationService.LogoutAsync(userId!);
            return NoContent();
        }
    }
}
