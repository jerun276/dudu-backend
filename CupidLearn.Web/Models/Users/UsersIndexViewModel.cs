namespace CupidLearn.Web.Models.Users;

public class UsersIndexViewModel
{
    public string? Error { get; set; }

    public string? Query { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = 20;

    public int Total { get; set; }
    public List<UserListItemVm> Items { get; set; } = [];

    public Guid? SelectedUserId { get; set; }
    public UserSummaryVm? Summary { get; set; }

    public record UserListItemVm(
        Guid UserId,
        string Email,
        string? PhoneNumber,
        string Role,
        string? FullName,
        DateTimeOffset CreatedAt);

    public record LimitsVm(string Plan, int MaxOrganizations, int MaxSeatsPerOrganization, int MaxChildren);

    public record SubscriptionVm(
        Guid Id,
        string Provider,
        string ProviderSubscriptionId,
        string Status,
        DateTimeOffset? CurrentPeriodStart,
        DateTimeOffset? CurrentPeriodEnd,
        DateTimeOffset UpdatedAt);

    public record SeatAssignmentVm(Guid SeatId, Guid OrganizationId, string SeatStatus);

    public record ChildVm(Guid Id, string DisplayName, int? Age, DateTimeOffset CreatedAt);

    public record UserSummaryVm(
        Guid UserId,
        string Email,
        string? PhoneNumber,
        string Role,
        string? FullName,
        string? DisplayName,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ProfileUpdatedAt,
        SubscriptionVm? Subscription,
        LimitsVm Limits,
        List<SeatAssignmentVm> SeatAssignments,
        List<ChildVm> Children);
}
