using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Application.Contracts.Content;

public record LevelResponse(Guid Id, string? Language, string? Name);

public record ModuleResponse(Guid Id, Guid LevelId, string? Name, int OrderIndex);

public record ExamResponse(Guid Id, Guid ModuleId, string? Title);

public record LevelCreateRequest(
    string? Language,
    [Required] string Name);

public record LevelUpdateRequest(
    string? Language,
    [Required] string Name);

public record ModuleCreateRequest(
    [Required] string Name,
    int OrderIndex = 0);

public record ModuleUpdateRequest(
    [Required] string Name,
    int OrderIndex = 0);

public record ExamCreateRequest(
    [Required] string Title);
