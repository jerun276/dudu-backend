using CupidLearn.Application.Contracts.Content;

namespace CupidLearn.Application.Abstractions;

public interface IContentAdminService
{
    Task<ActivityTypeResponse> CreateActivityTypeAsync(ActivityTypeCreateRequest request, CancellationToken ct);

    Task<ActivityTypeResponse> UpdateActivityTypeAsync(Guid activityTypeId, ActivityTypeUpdateRequest request, CancellationToken ct);

    Task DeleteActivityTypeAsync(Guid activityTypeId, CancellationToken ct);

    Task<ModuleResponse> CreateModuleAsync(ModuleCreateV2Request request, CancellationToken ct);

    Task<ModuleResponse> UpdateModuleAsync(Guid moduleId, ModuleUpdateRequest request, CancellationToken ct);

    Task DeleteModuleAsync(Guid moduleId, CancellationToken ct);

    Task<LessonResponse> CreateLessonAsync(Guid moduleId, LessonCreateRequest request, CancellationToken ct);

    Task<LessonResponse> UpdateLessonAsync(Guid lessonId, LessonUpdateRequest request, CancellationToken ct);

    Task DeleteLessonAsync(Guid lessonId, CancellationToken ct);

    Task<ActivityResponse> CreateActivityAsync(Guid lessonId, ActivityCreateRequest request, CancellationToken ct);

    Task<ActivityResponse> UpdateActivityAsync(Guid activityId, ActivityUpdateRequest request, CancellationToken ct);

    Task DeleteActivityAsync(Guid activityId, CancellationToken ct);

    Task<QuizResponse> CreateQuizAsync(Guid lessonId, QuizCreateRequest request, CancellationToken ct);

    Task<QuizResponse> UpdateQuizAsync(Guid quizId, QuizUpdateRequest request, CancellationToken ct);

    Task DeleteQuizAsync(Guid quizId, CancellationToken ct);

    Task<QuizQuestionResponse> CreateQuizQuestionAsync(Guid quizId, QuizQuestionCreateRequest request, CancellationToken ct);

    Task<QuizQuestionResponse> UpdateQuizQuestionAsync(Guid questionId, QuizQuestionUpdateRequest request, CancellationToken ct);

    Task DeleteQuizQuestionAsync(Guid questionId, CancellationToken ct);
}
