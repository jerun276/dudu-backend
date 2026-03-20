using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class CreateLevelViewModel
{
    [Required]
    public string? Language { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}
