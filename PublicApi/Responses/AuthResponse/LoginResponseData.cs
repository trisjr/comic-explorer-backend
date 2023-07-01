namespace PublicApi.Responses.AuthResponse;

public class LoginResponseData
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
}
