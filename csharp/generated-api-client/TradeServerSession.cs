namespace GeneratedApiClient;

/// <summary>
/// The secrets <see cref="SigningHandler"/> needs. Until <see cref="SignIn"/> is called, requests are signed
/// with the password, which only the authorize request should use.
/// </summary>
public class TradeServerSession(string password)
{
    private volatile Keys current = new(null, password);

    public Keys Current => current;

    public void SignIn(string apiKey, string signingToken) => current = new(apiKey, signingToken);

    public record Keys(string? ApiKey, string SigningSecret);
}
