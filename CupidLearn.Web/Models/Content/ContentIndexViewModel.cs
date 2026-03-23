namespace CupidLearn.Web.Models.Content;

public class ContentIndexViewModel
{
    public string? Error { get; set; }

    public string? Language { get; set; }

    public List<ActivityTypeVm> ActivityTypes { get; set; } = [];

    public List<LevelVm> Levels { get; set; } = [];

    public Guid? SelectedLevelId { get; set; }
    public List<ModuleVm> Modules { get; set; } = [];

    public Guid? SelectedModuleId { get; set; }
    public List<LessonVm> Lessons { get; set; } = [];

    public Guid? SelectedLessonId { get; set; }
    public List<ActivityVm> Activities { get; set; } = [];

    public Guid? SelectedQuizId { get; set; }
    public List<QuizVm> Quizzes { get; set; } = [];
    public List<QuizQuestionVm> QuizQuestions { get; set; } = [];

    public record ActivityTypeVm(Guid Id, string Key, string DisplayName, string? Description, string? SchemaJson);

    public record LevelVm(Guid Id, string Language, string Name);

    public record ModuleVm(Guid Id, Guid LevelId, string Name, int OrderIndex);

    public record LessonVm(Guid Id, Guid ModuleId, string Title, string? Description, int OrderIndex);

    public record ActivityVm(Guid Id, Guid LessonId, string Type, string Title, string? ImageUrl, int OrderIndex, string? PayloadJson);

    public record QuizVm(Guid Id, Guid LessonId, string Title);

    public record QuizQuestionVm(Guid Id, Guid QuizId, string Prompt, string OptionA, string OptionB, string OptionC, string OptionD, string CorrectOption);
}
