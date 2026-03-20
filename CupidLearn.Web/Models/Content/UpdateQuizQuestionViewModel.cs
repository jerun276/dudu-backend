using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class UpdateQuizQuestionViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid QuizId { get; set; }

    [Required]
    public string Prompt { get; set; } = string.Empty;

    [Required]
    public string OptionA { get; set; } = string.Empty;

    [Required]
    public string OptionB { get; set; } = string.Empty;

    [Required]
    public string OptionC { get; set; } = string.Empty;

    [Required]
    public string OptionD { get; set; } = string.Empty;

    [Required]
    public string CorrectOption { get; set; } = "A";

    public string? Language { get; set; }

    public Guid? LevelId { get; set; }

    public Guid? ModuleId { get; set; }

    public Guid? LessonId { get; set; }
}
