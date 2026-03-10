using CupidLearn.Application.Abstractions;
using CupidLearn.Application.Contracts.Billing;
using CupidLearn.Application.Exceptions;
using CupidLearn.Domain.Billing;
using CupidLearn.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CupidLearn.Infrastructure.Services;

public class OrganizationSeatService(AppDbContext db) : IOrganizationSeatService
{
    public async Task<CreateOrganizationResponse> CreateOrganizationAsync(Guid authUserId, string? authRole, CreateOrganizationRequest request, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("name is required");
        }

        var org = new Organization
        {
            Name = request.Name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        db.Organizations.Add(org);

        var seatCount = request.SeatCount.GetValueOrDefault(0);
        if (seatCount < 0)
        {
            throw new BadRequestException("seatCount must be >= 0");
        }

        for (var i = 0; i < seatCount; i++)
        {
            db.Seats.Add(new Seat
            {
                OrganizationId = org.Id,
                Status = SeatStatus.AVAILABLE,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);

        return new CreateOrganizationResponse(org.Id, org.Name, org.CreatedAt, org.UpdatedAt);
    }

    public async Task<List<SeatResponse>> ListSeatsAsync(Guid authUserId, string? authRole, Guid organizationId, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == organizationId, ct);
        if (!orgExists)
        {
            throw new NotFoundException("Organization not found");
        }

        var seats = await db.Seats
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);

        return seats.Select(ToResponse).ToList();
    }

    public async Task<SeatResponse> AssignSeatAsync(Guid authUserId, string? authRole, Guid organizationId, Guid userId, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        var orgExists = await db.Organizations.AnyAsync(x => x.Id == organizationId, ct);
        if (!orgExists)
        {
            throw new NotFoundException("Organization not found");
        }

        var userExists = await db.Users.AnyAsync(x => x.Id == userId, ct);
        if (!userExists)
        {
            throw new NotFoundException("User not found");
        }

        var seat = await db.Seats
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.Status == SeatStatus.AVAILABLE, ct);

        if (seat == null)
        {
            throw new ConflictException("No available seats");
        }

        seat.UserId = userId;
        seat.Status = SeatStatus.ASSIGNED;
        seat.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return ToResponse(seat);
    }

    public async Task<SeatResponse> RevokeSeatAsync(Guid authUserId, string? authRole, Guid organizationId, Guid userId, CancellationToken ct)
    {
        EnsureAdmin(authRole);

        var seat = await db.Seats.FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.UserId == userId && x.Status == SeatStatus.ASSIGNED, ct);
        if (seat == null)
        {
            throw new NotFoundException("Seat assignment not found");
        }

        seat.Status = SeatStatus.REVOKED;
        seat.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return ToResponse(seat);
    }

    private static SeatResponse ToResponse(Seat s) => new(
        s.Id,
        s.OrganizationId,
        s.UserId,
        s.Status,
        s.CreatedAt,
        s.UpdatedAt);

    private static void EnsureAdmin(string? authRole)
    {
        var isAdmin = string.Equals(authRole, "ADMIN", StringComparison.OrdinalIgnoreCase);
        if (!isAdmin)
        {
            throw new ForbiddenException("Forbidden");
        }
    }
}
