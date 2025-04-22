using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Net.WebSockets;
using ApiExample;

const int login = 1;
const string password = "xxx";
const string tradeServerAdminApiUrl = "https://yourbourse.trade:2xxxx";

var user = new AuthUser { ApiKey = "", Password = password };

Console.WriteLine("================================== Authorization (Nonce) ==================================");
// Api examples (Nonce)
var authNonceResponse = await Authorise(login, AuthenticationMethod.Nonce);
user.ApiKey = authNonceResponse!.Token;

return;

async Task<ApiToken?> Authorise(int login, AuthenticationMethod authenticationMethod)
{
    var payload = new AuthRequest { Login = login };

    using var client = new HttpClient();

    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(tradeServerAdminApiUrl), "/api/v1/authorize"))
    {
        Content = new StringContent(JsonSerializer.Serialize(payload, ApiHeaders.JsonSerializerOptions),
            Encoding.UTF8, "application/json")
    };

    var headers = ApiHeaders.GetPostHeaders(user, payload, authenticationMethod);
    foreach (var header in headers)
    {
        request.Headers.Add(header.Key, header.Value);
    }

    var response = await client.SendAsync(request);
    var responseContent = await response.Content.ReadAsStringAsync();

    Console.WriteLine("Response:");
    Console.WriteLine(responseContent);

    return await response.Content.ReadFromJsonAsync<ApiToken>();
}

static string GenerateRandomUppercaseString(int length)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    Random random = new Random();
    char[] result = new char[length];

    for (int i = 0; i < length; i++)
    {
        result[i] = chars[random.Next(chars.Length)];
    }

    return new string(result);
}
