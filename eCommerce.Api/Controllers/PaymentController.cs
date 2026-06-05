using eCommerce.Application.DTOs.Orders;
using eCommerce.Application.Services.Interfaces.Orders;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController(IPaymentMethodService paymentMethodService) : ControllerBase
    {
        [HttpGet("methods")]
        public async Task<ActionResult<IEnumerable<GetPaymentMethodDTO>>> GetMethodsAsync()
        {
            IEnumerable<GetPaymentMethodDTO> paymentMethods = await paymentMethodService.GetMethodsAsync();
            if(!paymentMethods.Any())
                return NotFound("No payment methods found.");
            else
                return Ok(paymentMethods);
        }
    }
}
