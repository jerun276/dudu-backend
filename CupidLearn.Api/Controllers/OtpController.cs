using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;

namespace CupidLearn.Api.Controllers;

[ApiController]
[Route("api/auth/otp")]
public class OtpController(IOtpService otpService) : ControllerBase
{
    [HttpPost("request")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponse>> RequestOtp([FromBody] OtpRequest request, CancellationToken ct)
    {
        var response = await otpService.RequestAsync(request, ct);
        return Ok(response);
    }

    [HttpPost("verify")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponse>> Verify([FromBody] OtpVerifyRequest request, CancellationToken ct)
    {
        var response = await otpService.VerifyAsync(request, ct);
        return Ok(response);
    }
}
