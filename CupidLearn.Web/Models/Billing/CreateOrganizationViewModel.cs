using System.ComponentModel.DataAnnotations;

namespace CupidLearn.Web.Models.Billing;

public class CreateOrganizationViewModel
{
    [Required]
    public string Name { get; set; } = "";

    public int? SeatCount { get; set; }
}
