using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace GeneratedApiClient;

/// <summary>
/// Adds the API key to every request, and a timestamp HMAC signature to every POST, PUT and DELETE.
/// </summary>
public class SigningHandler(TradeServerSession session) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var keys = session.Current;
        if (keys.ApiKey is not null)
        {
            SetHeader(request, "X-YB-API-Key", keys.ApiKey);
        }

        if (request.Method != HttpMethod.Get)
        {
            var body = request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken);
            var timestamp = ((DateTime.UtcNow - DateTime.UnixEpoch).Ticks / TimeSpan.TicksPerMicrosecond).ToString(CultureInfo.InvariantCulture);
            var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(keys.SigningSecret), Encoding.UTF8.GetBytes($"Content={body}\nTimestamp={timestamp}"));

            SetHeader(request, "X-YB-Timestamp", timestamp);
            SetHeader(request, "X-YB-Sign", Base64Url.EncodeToString(signature));
        }

        return await base.SendAsync(request, cancellationToken);
    }

    // A retried request arrives here again with the headers from the previous attempt.
    private static void SetHeader(HttpRequestMessage request, string name, string value)
    {
        request.Headers.Remove(name);
        request.Headers.Add(name, value);
    }
}
