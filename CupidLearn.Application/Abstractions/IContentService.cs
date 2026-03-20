using CupidLearn.Application.Contracts.Content;

namespace CupidLearn.Application.Abstractions;

public interface IContentService
{
    Task<List<LevelResponse>> ListLevelsAsync(CancellationToken ct);

    Task<List<ModuleResponse>> ListModulesByLevelAsync(Guid levelId, CancellationToken ct);

    Task<List<ExamResponse>> ListExamsByModuleAsync(Guid moduleId, CancellationToken ct);

    Task<LevelResponse> CreateLevelAsync(LevelCreateRequest request, CancellationToken ct);

    Task<LevelResponse> UpdateLevelAsync(Guid levelId, LevelUpdateRequest request, CancellationToken ct);

    Task DeleteLevelAsync(Guid levelId, CancellationToken ct);

    Task<ModuleResponse> CreateModuleAsync(Guid levelId, ModuleCreateRequest request, CancellationToken ct);

    Task<ExamResponse> CreateExamAsync(Guid moduleId, ExamCreateRequest request, CancellationToken ct);
}
