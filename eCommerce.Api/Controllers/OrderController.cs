using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
        [Authorize(Roles = "User,Influencer")]
        [EnableRateLimiting("api")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] CreateOrderDTO order)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await orderService.CreateOrderAsync(userId, order);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }
        
        [HttpPost("checkout")]
        [Authorize(Roles = "User,Influencer")]
        public async Task<IActionResult> CheckoutAsync([FromBody] CheckoutDTO checkout)
        {

            ServiceResponse result = await orderService.CheckoutAsync(checkout);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }
        
        [HttpPut("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatusAsync([FromBody]UpdateOrderStatusDTO request)
        {
            var result = await orderService.UpdateStatusAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync([FromQuery] OrderFilterDTO filter)
        {
            var orders = await orderService.GetAllAsync(filter);
            return Ok(orders);
        }

        [HttpGet("byId/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var orders = await orderService.GetByIdAsync(id);
            return orders is null ? NotFound() : Ok(orders);
        }
    }
}
