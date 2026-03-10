using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class CreateQuizQuestionViewModel
{
    [Required]
    public Guid QuizId { get; set; }

    [Required]
    public string Prompt { get; set; } = "";

    [Required]
    public string OptionA { get; set; } = "";

    [Required]
    public string OptionB { get; set; } = "";

    [Required]
    public string OptionC { get; set; } = "";

    [Required]
    public string OptionD { get; set; } = "";

    [Required]
    public string CorrectOption { get; set; } = "A";
}
