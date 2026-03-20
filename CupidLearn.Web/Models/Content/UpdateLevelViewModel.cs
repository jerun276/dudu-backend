using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class UpdateLevelViewModel
{
    [Required]
    public Guid Id { get; set; }

    public string? Language { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}
