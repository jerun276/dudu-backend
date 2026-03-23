using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Content;

public class CreateModuleViewModel
{
    [Required]
    public Guid LevelId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int OrderIndex { get; set; }
}
