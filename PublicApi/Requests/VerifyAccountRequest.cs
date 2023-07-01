using System.ComponentModel.DataAnnotations;

namespace PublicApi.Requests;

public class VerifyAccountRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
}
