using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class CreateLessonViewModel
{
    [Required]
    public Guid ModuleId { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Content { get; set; }

    public int OrderIndex { get; set; }
}
