using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Api.Dtos;

public record ProfileUpsertRequest(
    string? DisplayName,
    string? FullName,
    string? AvatarUrl,
    string? Locale,
    string? Country,
    string? Province);

public record ProfileResponse(
    Guid Id,
    Guid UserId,
    string? DisplayName,
    string? FullName,
    string? AvatarUrl,
    string? Locale,
    string? Country,
    string? Province,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record ChildProfileCreateRequest(
    [Required] string DisplayName,
    int? Age);

public record ChildProfileResponse(
    Guid Id,
    Guid ParentUserId,
    string DisplayName,
    int? Age,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
