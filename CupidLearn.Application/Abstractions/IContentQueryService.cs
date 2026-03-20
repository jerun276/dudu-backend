using CupidLearn.Application.Contracts.Content;

namespace CupidLearn.Application.Abstractions;

public interface IContentQueryService
{
    Task<List<ActivityTypeResponse>> ListActivityTypesAsync(CancellationToken ct);

    Task<List<LevelResponse>> ListLevelsAsync(string? language, CancellationToken ct);

    Task<LevelResponse> GetLevelByIdAsync(Guid levelId, CancellationToken ct);

    Task<List<ModuleResponse>> ListModulesByLevelAsync(Guid levelId, CancellationToken ct);

    Task<ModuleResponse> GetModuleByIdAsync(Guid moduleId, CancellationToken ct);

    Task<List<LessonResponse>> ListLessonsByModuleAsync(Guid moduleId, CancellationToken ct);

    Task<LessonResponse> GetLessonByIdAsync(Guid lessonId, CancellationToken ct);

    Task<List<ActivityResponse>> ListActivitiesByLessonAsync(Guid lessonId, CancellationToken ct);

    Task<ActivityResponse> GetActivityByIdAsync(Guid activityId, CancellationToken ct);

    Task<List<QuizResponse>> ListQuizzesByLessonAsync(Guid lessonId, CancellationToken ct);

    Task<List<QuizQuestionResponse>> ListQuestionsByQuizAsync(Guid quizId, CancellationToken ct);

    Task<QuizResponse> GetQuizByIdAsync(Guid quizId, CancellationToken ct);
}
