namespace CupidLearn.Application.Contracts.Admin;

public record AdminUserListItemResponse(
    Guid UserId,
    string Email,
    string? PhoneNumber,
    string Role,
    string? FullName,
    DateTimeOffset CreatedAt);

public record AdminUserSearchResponse(
    int Total,
    List<AdminUserListItemResponse> Items);

public record AdminSeatAssignmentResponse(
    Guid SeatId,
    Guid OrganizationId,
    string SeatStatus);

public record AdminChildResponse(
    Guid Id,
    string DisplayName,
    DateTimeOffset CreatedAt);

public record AdminSubscriptionResponse(
    Guid Id,
    string Provider,
    string ProviderSubscriptionId,
    string Status,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    DateTimeOffset UpdatedAt);

public record AdminSubscriptionLimitsResponse(
    string Plan,
    int MaxOrganizations,
    int MaxSeatsPerOrganization,
    int MaxChildren);

public record AdminUserSummaryResponse(
    Guid UserId,
    string Email,
    string? PhoneNumber,
    string Role,
    string? FullName,
    string? DisplayName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProfileUpdatedAt,
    AdminSubscriptionResponse? Subscription,
    AdminSubscriptionLimitsResponse Limits,
    List<AdminSeatAssignmentResponse> SeatAssignments,
    List<AdminChildResponse> Children);
