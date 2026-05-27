using System.Security.Cryptography;
using System.Text;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Auth;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Profiles;
using CupidLearn.Domain.Users;
using CupidLearn.Infrastructure.Auth;
using CupidLearn.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class AuthService(AppDbContext db, JwtTokenService jwtTokenService, IEmailSender emailSender) : IAuthService
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existing = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (existing != null)
        {
            throw new ConflictException("Email already registered");
        }

        var user = new AppUser
        {
            Email = email,
            PhoneNo = request.PhoneNo,
            Role = string.IsNullOrWhiteSpace(request.Role) ? "PARENT" : request.Role.Trim().ToUpperInvariant(),
            IsVerified = false
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        db.UserProfiles.Add(new UserProfile { UserId = user.Id });

        await db.SaveChangesAsync(ct);

        var accessToken = jwtTokenService.CreateAccessToken(user.Id, user.Role);
        var refreshToken = await IssueRefreshTokenAsync(user.Id, ct);
        return new AuthResponse(user.Id, user.Email, user.Role, accessToken, refreshToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException("Invalid credentials");
        }

        var accessToken = jwtTokenService.CreateAccessToken(user.Id, user.Role);
        var refreshToken = await IssueRefreshTokenAsync(user.Id, ct);
        return new AuthResponse(user.Id, user.Email, user.Role, accessToken, refreshToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new BadRequestException("refreshToken is required");
        }

        var oldTokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == oldTokenHash, ct);
        if (token == null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        if (token.RevokedAt.HasValue)
        {
            throw new UnauthorizedException("Refresh token revoked");
        }

        if (token.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedException("Refresh token expired");
        }

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == token.UserId, ct);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid refresh token");
        }

        var newRefreshToken = jwtTokenService.CreateRefreshToken();
        var newHash = jwtTokenService.HashRefreshToken(newRefreshToken);
        var now = DateTimeOffset.UtcNow;

        token.RevokedAt = now;
        token.ReplacedByTokenHash = newHash;

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newHash,
            ExpiresAt = jwtTokenService.GetRefreshTokenExpiresAt(now),
            CreatedAt = now
        });

        await db.SaveChangesAsync(ct);

        var accessToken = jwtTokenService.CreateAccessToken(user.Id, user.Role);
        return new AuthResponse(user.Id, user.Email, user.Role, accessToken, newRefreshToken);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new BadRequestException("refreshToken is required");
        }

        var tokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);
        if (token == null)
        {
            return;
        }

        if (!token.RevokedAt.HasValue)
        {
            token.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
        }
    }

    private async Task<string> IssueRefreshTokenAsync(Guid userId, CancellationToken ct)
    {
        var refreshToken = jwtTokenService.CreateRefreshToken();
        var hash = jwtTokenService.HashRefreshToken(refreshToken);

        var now = DateTimeOffset.UtcNow;
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = hash,
            ExpiresAt = jwtTokenService.GetRefreshTokenExpiresAt(now),
            CreatedAt = now
        });

        await db.SaveChangesAsync(ct);

        return refreshToken;
    }

    public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user == null)
        {
            return new MessageResponse("If the account exists, a reset OTP has been sent.");
        }

        var active = await db.UserOtps
            .Where(x => x.UserId == user.Id && !x.IsVerified && x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (active != null)
        {
            throw new BadRequestException("OTP already issued. Please wait before requesting again.");
        }

        var otp = RandomNumberGenerator.GetInt32(1000, 9999).ToString();
        var otpHash = HashOtp(otp);

        db.UserOtps.Add(new UserOtp
        {
            UserId = user.Id,
            OtpHash = otpHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            IsVerified = false
        });

        await db.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            toEmail: email,
            subject: "CupidLearn Password Reset",
            bodyText: $"Your password reset OTP is: {otp}. It will expire in 10 minutes.",
            ct: ct);

        return new MessageResponse("If the account exists, a reset OTP has been sent.");
    }

    public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user == null)
        {
            throw new BadRequestException("Invalid or expired OTP");
        }

        var otpHash = HashOtp(request.Otp);

        var otpRow = await db.UserOtps
            .Where(x => x.UserId == user.Id && !x.IsVerified && x.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (otpRow == null || otpRow.OtpHash != otpHash)
        {
            throw new BadRequestException("Invalid or expired OTP");
        }

        otpRow.IsVerified = true;
        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return new MessageResponse("Password reset successfully.");
    }

    private static string HashOtp(string otp)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes);
    }
}
