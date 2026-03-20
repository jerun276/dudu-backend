using CupidLearn.Application.Contracts.Auth;

namespace CupidLearn.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);

    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct);

    Task LogoutAsync(LogoutRequest request, CancellationToken ct);
}
