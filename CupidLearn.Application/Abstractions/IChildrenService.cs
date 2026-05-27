using CupidLearn.Application.Contracts.Profiles;

namespace CupidLearn.Application.Abstractions;

public interface IChildrenService
{
    Task<ChildProfileResponse> CreateAsync(Guid parentUserId, ChildProfileCreateRequest request, CancellationToken ct);

    Task<ChildProfileResponse> UpdateAsync(Guid parentUserId, Guid childId, ChildProfileUpdateRequest request, CancellationToken ct);

    Task<List<ChildProfileResponse>> ListAsync(Guid parentUserId, CancellationToken ct);
}
