using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CupidLearn.Application.Contracts.Content;

public record ModuleCreateV2Request(
    [Required] Guid LevelId,
    [Required] string Name);

public record LessonCreateRequest(
    [Required] string Title,
    string? Description,
    string? Content,
    int OrderIndex);

public record LessonUpdateRequest(
    [Required] string Title,
    string? Description,
    string? Content,
    int OrderIndex);

public record LessonResponse(
    Guid Id,
    Guid ModuleId,
    string Title,
    string? Description,
    string? Content,
    int OrderIndex,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record ActivityCreateRequest(
    [Required] string Type,
    [Required] string Title,
    string? ImageUrl,
    JsonElement? Payload,
    int OrderIndex);

public record ActivityUpdateRequest(
    [Required] string Type,
    [Required] string Title,
    string? ImageUrl,
    JsonElement? Payload,
    int OrderIndex);

public record ActivityResponse(
    Guid Id,
    Guid LessonId,
    string Type,
    string Title,
    string? ImageUrl,
    JsonElement? Payload,
    int OrderIndex,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record QuizCreateRequest([Required] string Title);

public record QuizResponse(Guid Id, Guid LessonId, string Title);

public record QuizQuestionCreateRequest(
    [Required] string Prompt,
    [Required] string OptionA,
    [Required] string OptionB,
    [Required] string OptionC,
    [Required] string OptionD,
    [Required] string CorrectOption);

public record QuizQuestionResponse(
    Guid Id,
    Guid QuizId,
    string Prompt,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectOption,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
