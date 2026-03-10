using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet("/health")]
    public IActionResult Health() => Ok(new { status = "UP" });
}
