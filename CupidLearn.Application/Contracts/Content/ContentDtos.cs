using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Application.Contracts.Content;

public record LevelResponse(Guid Id, string? Language, string? Name);

public record ModuleResponse(Guid Id, Guid LevelId, string? Name);

public record ExamResponse(Guid Id, Guid ModuleId, string? Title);

public record LevelCreateRequest(
    string? Language,
    [Required] string Name);

public record ModuleCreateRequest(
    [Required] string Name);

public record ExamCreateRequest(
    [Required] string Title);
