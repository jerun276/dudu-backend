using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class UpdateActivityTypeViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? SchemaJson { get; set; }

    public string? Language { get; set; }

    public Guid? LevelId { get; set; }

    public Guid? ModuleId { get; set; }

    public Guid? LessonId { get; set; }

    public Guid? QuizId { get; set; }
}
