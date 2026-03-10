namespace CupidLearn.Web.Models.Users;

public class UsersIndexViewModel
{
    public string? Error { get; set; }

    public Guid? UserId { get; set; }
    public ProfileVm? Profile { get; set; }

    public record ProfileVm(
        Guid UserId,
        string? FullName,
        string? Email,
        string? PhoneNumber,
        string? Role,
        DateTimeOffset UpdatedAt);
}
