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

public class AuthService(AppDbContext db, JwtTokenService jwtTokenService) : IAuthService
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

        var token = jwtTokenService.CreateAccessToken(user.Id, user.Role);
        return new AuthResponse(user.Id, user.Email, user.Role, token);
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

        var token = jwtTokenService.CreateAccessToken(user.Id, user.Role);
        return new AuthResponse(user.Id, user.Email, user.Role, token);
    }
}
