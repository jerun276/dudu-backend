using CupidLearn.Application.Contracts.Content;

namespace CupidLearn.Application.Abstractions;

public interface IContentAdminService
{
    Task<ModuleResponse> CreateModuleAsync(ModuleCreateV2Request request, CancellationToken ct);

    Task<LessonResponse> CreateLessonAsync(Guid moduleId, LessonCreateRequest request, CancellationToken ct);

    Task<LessonResponse> UpdateLessonAsync(Guid lessonId, LessonUpdateRequest request, CancellationToken ct);

    Task DeleteLessonAsync(Guid lessonId, CancellationToken ct);

    Task<ActivityResponse> CreateActivityAsync(Guid lessonId, ActivityCreateRequest request, CancellationToken ct);

    Task<ActivityResponse> UpdateActivityAsync(Guid activityId, ActivityUpdateRequest request, CancellationToken ct);

    Task DeleteActivityAsync(Guid activityId, CancellationToken ct);

    Task<QuizResponse> CreateQuizAsync(Guid lessonId, QuizCreateRequest request, CancellationToken ct);

    Task<QuizQuestionResponse> CreateQuizQuestionAsync(Guid quizId, QuizQuestionCreateRequest request, CancellationToken ct);
}
