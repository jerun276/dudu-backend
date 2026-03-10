using CupidLearn.Application.Contracts.Billing;

namespace CupidLearn.Application.Abstractions;

public interface IOrganizationSeatService
{
    Task<CreateOrganizationResponse> CreateOrganizationAsync(Guid authUserId, string? authRole, CreateOrganizationRequest request, CancellationToken ct);

    Task<List<SeatResponse>> ListSeatsAsync(Guid authUserId, string? authRole, Guid organizationId, CancellationToken ct);

    Task<SeatResponse> AssignSeatAsync(Guid authUserId, string? authRole, Guid organizationId, Guid userId, CancellationToken ct);

    Task<SeatResponse> RevokeSeatAsync(Guid authUserId, string? authRole, Guid organizationId, Guid userId, CancellationToken ct);
}
