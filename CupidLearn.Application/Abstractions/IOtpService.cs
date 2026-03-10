using CupidLearn.Application.Contracts.Auth;

namespace CupidLearn.Application.Abstractions;

public interface IOtpService
{
    Task<MessageResponse> RequestAsync(OtpRequest request, CancellationToken ct);

    Task<MessageResponse> VerifyAsync(OtpVerifyRequest request, CancellationToken ct);
}
