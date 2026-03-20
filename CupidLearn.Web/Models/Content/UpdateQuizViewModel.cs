using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class UpdateQuizViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid LessonId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Language { get; set; }

    public Guid? LevelId { get; set; }

    public Guid? ModuleId { get; set; }
}
