using CupidLearn.Application.Contracts.Profiles;

namespace CupidLearn.Application.Abstractions;

public interface IProfilesService
{
    Task<ProfileResponse> GetByUserIdAsync(Guid authUserId, string? authRole, Guid userId, CancellationToken ct);

    Task<ProfileResponse> UpsertAsync(Guid authUserId, Guid userId, ProfileUpsertRequest request, CancellationToken ct);
}
