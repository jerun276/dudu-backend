using System.ComponentModel.DataAnnotations;
using CupidLearn.Domain.Billing;

namespace CupidLearn.Application.Contracts.Billing;

public record SubscriptionUpsertRequest(
    [Required] string Provider,
    [Required] string ProviderSubscriptionId,
    [Required] SubscriptionStatus Status,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd);

public record SubscriptionResponse(
    Guid Id,
    Guid UserId,
    string Provider,
    string ProviderSubscriptionId,
    SubscriptionStatus Status,
    DateTimeOffset? CurrentPeriodStart,
    DateTimeOffset? CurrentPeriodEnd,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record SubscriptionLimitsResponse(
    string Plan,
    int MaxOrganizations,
    int MaxSeatsPerOrganization,
    int MaxChildren);

public record CreateOrganizationRequest(
    [Required] string Name,
    int? SeatCount);

public record CreateOrganizationResponse(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record AssignSeatRequest([Required] Guid UserId);

public record RevokeSeatRequest([Required] Guid UserId);

public record SeatResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? UserId,
    SeatStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
