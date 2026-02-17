using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ApiExample;

public static class ApiHeaders
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    /// <summary>
    /// Get headers for authorization request. Uses password for signing.
    /// </summary>
    public static Dictionary<string, string> GetAuthorizationHeaders<T>(string password, T data, AuthenticationMethod authenticationMethod = AuthenticationMethod.Nonce)
    {
        return authenticationMethod == AuthenticationMethod.Timestamp
            ? GetAuthHeadersWithTimestamp(password, data)
            : GetAuthHeadersWithNonce(password, data);
    }
    
    /// <summary>
    /// Get headers for GET requests (requires active session).
    /// </summary>
    public static Dictionary<string, string> GetApiGetHeaders(AuthUser authUser)
    {
        return new Dictionary<string, string>
        {
            { "X-YB-API-Key", authUser.ApiKey }
        };
    }

    /// <summary>
    /// Get headers for POST requests (requires active session). Uses signingToken for signing.
    /// </summary>
    public static Dictionary<string, string> GetApiPostHeaders<T>(AuthUser authUser, T data, AuthenticationMethod authenticationMethod = AuthenticationMethod.Nonce)
    {
        return authenticationMethod == AuthenticationMethod.Timestamp
            ? GetApiHeadersWithTimestamp(authUser, data)
            : GetApiHeadersWithNonce(authUser, data);
    }

    private static Dictionary<string, string> GetAuthHeadersWithNonce<T>(string password, T data)
    {
        var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return new Dictionary<string, string>
        {
            { "X-YB-Nonce", nonce.ToString() },
            { "X-YB-Sign", GetHmacDigest(password, $"Content={JsonSerializer.Serialize(data, JsonSerializerOptions)}\nNonce={nonce}") }
        };
    }

    private static Dictionary<string, string> GetAuthHeadersWithTimestamp<T>(string password, T data)
    {
        var timestampInMicroseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000;

        return new Dictionary<string, string>
        {
            { "X-YB-Timestamp", timestampInMicroseconds.ToString() },
            { "X-YB-Sign", GetHmacDigest(password, $"Content={JsonSerializer.Serialize(data, JsonSerializerOptions)}\nTimestamp={timestampInMicroseconds}") }
        };
    }

    private static Dictionary<string, string> GetApiHeadersWithNonce<T>(AuthUser authUser, T data)
    {
        var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        return new Dictionary<string, string>
        {
            { "X-YB-Nonce", nonce.ToString() },
            { "X-YB-API-Key", authUser.ApiKey },
            { "X-YB-Sign", GetHmacDigest(authUser.SigningToken, $"Content={JsonSerializer.Serialize(data, JsonSerializerOptions)}\nNonce={nonce}") }
        };
    }

    private static Dictionary<string, string> GetApiHeadersWithTimestamp<T>(AuthUser authUser, T data)
    {
        var timestampInMicroseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000;

        return new Dictionary<string, string>
        {
            { "X-YB-Timestamp", timestampInMicroseconds.ToString() },
            { "X-YB-API-Key", authUser.ApiKey },
            { "X-YB-Sign", GetHmacDigest(authUser.SigningToken, $"Content={JsonSerializer.Serialize(data, JsonSerializerOptions)}\nTimestamp={timestampInMicroseconds}") }
        };
    }

    private static string GetHmacDigest(string secret, string body)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
        return Convert.ToBase64String(hash)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}