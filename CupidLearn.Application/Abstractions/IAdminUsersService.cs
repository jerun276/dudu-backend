using CupidLearn.Application.Contracts.Admin;

namespace CupidLearn.Application.Abstractions;

public interface IAdminUsersService
{
    Task<AdminUserSearchResponse> SearchAsync(Guid authUserId, string? authRole, string? query, int skip, int take, CancellationToken ct);

    Task<AdminUserSummaryResponse> GetSummaryAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct);
}
