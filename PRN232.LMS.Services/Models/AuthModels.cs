namespace PRN232.LMS.Services.Models.AuthModels;

public class LoginRequest
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class AuthResponseData
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}
