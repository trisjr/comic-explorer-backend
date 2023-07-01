using System.ComponentModel.DataAnnotations;

namespace ApplicationCore.Entities.User;

public class RefreshToken
{
    [Key]
    public string Token { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
}
