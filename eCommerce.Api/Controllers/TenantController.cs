using eCommerce.Infrastructure.MultiTenant;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Api.Controllers
{
    [ApiController]
    [Route("api/tenant")]
    public class TenantController(ITenantContext tenantContext) : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(tenantContext.Config);
    }
}
