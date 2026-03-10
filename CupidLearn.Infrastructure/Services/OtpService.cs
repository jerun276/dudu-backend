using System.Security.Cryptography;
using System.Text;
using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Auth;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Users;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class OtpService(AppDbContext db) : IOtpService
{
    public async Task<MessageResponse> RequestAsync(OtpRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user == null || user.IsVerified)
        {
            return new MessageResponse("If the account exists and is not verified, an OTP has been issued.");
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

        Console.WriteLine($"OTP for {email}: {otp}");

        return new MessageResponse("If the account exists and is not verified, an OTP has been issued.");
    }

    public async Task<MessageResponse> VerifyAsync(OtpVerifyRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
        if (user == null)
        {
            return new MessageResponse("Account verified successfully.");
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
        user.IsVerified = true;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return new MessageResponse("Account verified successfully.");
    }

    private static string HashOtp(string otp)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes);
    }
}
