using System.ComponentModel.DataAnnotations;
using ApplicationCore.Constants;

namespace PublicApi.Requests;

public class RequestEmailModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; } = string.Empty;

    public RequestMailType Type { get; set; } = RequestMailType.ConfirmEmail;
}
