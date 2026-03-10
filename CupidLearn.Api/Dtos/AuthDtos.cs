using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Api.Dtos;

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
    string AccessToken);

public record MessageResponse(string Message);

public record OtpRequest([Required, EmailAddress] string Email);

public record OtpVerifyRequest(
    [Required, EmailAddress] string Email,
    [Required] string Otp);
