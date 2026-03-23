using CupidLearn.Application.Contracts.Admin;

namespace CupidLearn.Application.Abstractions;

public interface IAdminUsersService
{
    Task<AdminUserSearchResponse> SearchAsync(Guid authUserId, string? authRole, string? query, int skip, int take, CancellationToken ct);

    Task<AdminUserSummaryResponse> GetSummaryAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct);

    Task<AdminChildResponse> CreateChildAsync(Guid authUserId, string? authRole, Guid parentUserId, AdminChildCreateRequest request, CancellationToken ct);

    Task<AdminChildResponse> UpdateChildAsync(Guid authUserId, string? authRole, Guid childId, AdminChildUpdateRequest request, CancellationToken ct);

    Task DeleteChildAsync(Guid authUserId, string? authRole, Guid childId, CancellationToken ct);
}
