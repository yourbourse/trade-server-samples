namespace ApiExample;

/// <summary>
/// Represents an authenticated user session with API credentials.
/// Contains only the tokens needed for API calls (no password).
/// </summary>
public class AuthUser
{
    public string ApiKey { get; set; } = null!;
    public string SigningToken { get; set; } = null!;
}
