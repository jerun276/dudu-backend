using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class CreateQuizViewModel
{
    [Required]
    public Guid LessonId { get; set; }

    [Required]
    public string Title { get; set; } = "";
}
