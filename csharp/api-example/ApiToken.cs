namespace ApiExample;

public class ApiToken
{
    public string Token { get; set; } = null!;
    public string SigningToken { get; set; } = null!;
    public long Expiration { get; set; }
}