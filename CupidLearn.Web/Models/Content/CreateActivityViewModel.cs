using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class CreateActivityViewModel
{
    [Required]
    public Guid LessonId { get; set; }

    [Required]
    public string Type { get; set; } = "";

    [Required]
    public string Title { get; set; } = "";

    public string? ImageUrl { get; set; }

    public int OrderIndex { get; set; }

    public string? PayloadJson { get; set; }
}
