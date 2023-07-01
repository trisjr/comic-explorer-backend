namespace PublicApi.Responses.AuthResponse;

public class RegisterResponseData
{
    public string ConfirmUrl { get; set; } = string.Empty;
    public long ExpiresIn { get; set; }
}
