using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Application.Contracts.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    string? PhoneNo,
    string? Role);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(
    Guid UserId,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken);

public record RefreshRequest([Required] string RefreshToken);

public record LogoutRequest([Required] string RefreshToken);

public record MessageResponse(string Message);

public record OtpRequest([Required, EmailAddress] string Email);

public record OtpVerifyRequest(
    [Required, EmailAddress] string Email,
    [Required] string Otp);

public record ForgotPasswordRequest([Required, EmailAddress] string Email);

public record ResetPasswordRequest(
    [Required, EmailAddress] string Email,
    [Required] string Otp,
    [Required, MinLength(6)] string NewPassword);
