using System.ComponentModel.DataAnnotations;

namespace PublicApi.Requests;

public class LoginRequest
{
    [EmailAddress] [Required] public string Email { get; set; } = string.Empty;

    [Required] public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}
