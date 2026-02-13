using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ApiExample;

public static class ApiHeaders
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    
    public static Dictionary<string, string> GetGetHeaders(AuthUser user)
    {
        return new Dictionary<string, string>
        {
            { "X-YB-API-Key", user?.ApiKey ?? string.Empty }
        };
    }

    public static Dictionary<string, string> GetPostHeaders<T>(AuthUser user, T data, AuthenticationMethod authenticationMethod = AuthenticationMethod.Nonce)
    {
        return authenticationMethod == AuthenticationMethod.Timestamp
            ? GetPostHeadersWithTimestamp(user, data)
            : GetPostHeadersWithNonce(user, data);
    }

    private static Dictionary<string, string> GetPostHeadersWithNonce<T>(AuthUser user, T data)
    {
        var nonce = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var secret = user?.SigningToken ?? user?.Password ?? string.Empty;

        return new Dictionary<string, string>
        {
            { "X-YB-Nonce", nonce.ToString() },
            { "X-YB-API-Key", user?.ApiKey ?? string.Empty },
            { "X-YB-Sign", GetHmacDigest(secret, $"Content={JsonSerializer.Serialize(data, JsonSerializerOptions)}\nNonce={nonce}") }
        };
    }

    private static Dictionary<string, string> GetPostHeadersWithTimestamp<T>(AuthUser user, T data)
    {
        var timestampInMicroseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000;
        var secret = user?.SigningToken ?? user?.Password ?? string.Empty;

        return new Dictionary<string, string>
        {
            { "X-YB-Timestamp", timestampInMicroseconds.ToString() },
            { "X-YB-API-Key", user?.ApiKey ?? string.Empty },
            { "X-YB-Sign", GetHmacDigest(secret, $"Content={JsonSerializer.Serialize(data, JsonSerializerOptions)}\nTimestamp={timestampInMicroseconds}") }
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