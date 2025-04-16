using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Net.WebSockets;
using ApiExample;

const int login = 1;
const string password = "xxx";
const string tradeServerUrl = "https://yourbourse.trade:2xxxx";
const string wsUrl = "wss://yourbourse.trade:3xxxx/ws/v1";

var user = new AuthUser { ApiKey = "", Password = password };
var symbol = new Symbol
{
    Version = 0,
    Name = GenerateRandomUppercaseString(6),
    Path = GenerateRandomUppercaseString(6),
    Description = GenerateRandomUppercaseString(10),
    BaseCurrency = "PLN",
    ProfitCurrency = "PLN",
    MarginCurrency = "GBP"
};
Console.WriteLine("================================== Authorization (Nonce) ==================================");
// Api examples (Nonce)
var authNonceResponse = await Authorise(login, AuthenticationMethod.Nonce);
user.ApiKey = authNonceResponse!.Token;

Console.WriteLine("================================== Add Symbol (Nonce) ==================================");
await AddSymbol(symbol, AuthenticationMethod.Nonce);

Console.WriteLine("================================== Get Symbols (Nonce) ==================================");
await QuerySymbols(AuthenticationMethod.Nonce);
user.ApiKey = "";

Console.WriteLine("================================== Authorization (Timestamp) ==================================");
// Api examples (Timestamp)
var authTimestampResponse = await Authorise(login, AuthenticationMethod.Timestamp);
user.ApiKey = authTimestampResponse!.Token;

Console.WriteLine("================================== Add Symbol (Timestamp) ==================================");
await AddSymbol(symbol, AuthenticationMethod.Timestamp);

Console.WriteLine("================================== Get Symbols (Timestamp) ==================================");
await QuerySymbols(AuthenticationMethod.Timestamp);

Console.WriteLine("================================== Web Socket connect ==================================");
// WebSocket examples
var socket = new ClientWebSocket();
await socket.ConnectAsync(new Uri(tradeServerPublicWsUrl), CancellationToken.None);
Console.WriteLine($"WebSocket state after connect: {socket.State}");

// Start listening in a separate task
var listenTask = Task.Run(async () => await WebSockerHelpers.ListenWebSocket(socket, CancellationToken.None));
Console.WriteLine("WebSocket listener started...");

Console.WriteLine("================================== Web Socket (Ping) ==================================");
// Send ping message
var pingMessageJson =
    $$"""
      {
        "m": "ping",
        "h": {
          "X-YB-API-Key": "{{user.ApiKey}}",
          "X-YB-LOCALE": "en"
        },
        "reqId": "777816"
      }
      """;
await WebSockerHelpers.SendAsync(socket, pingMessageJson);
await Task.Delay(1000);

Console.WriteLine("================================== Web Socket (Subscribe L1) ==================================");
// Send subscribe message
var subscribeMessageJson =
    $$"""
      {
          "m": "subscribe",
          "c": "L1",
          "p": {
              "s": "EURUSD",
              "streaming": false
          },
          "h": {
              "X-YB-API-Key": "{{user.ApiKey}}",
              "X-YB-LOCALE": "en"
          },
          "reqId": "777816"
      }
      """;
await WebSockerHelpers.SendAsync(socket, subscribeMessageJson);
await Task.Delay(1000);

Console.WriteLine("================================== Web Socket (Unsubscribe L1) ==================================");
// Send unsubscribe message
var unsubscribeMessageJson =
    $$"""
      {
          "m": "unsubscribe",
          "c": "L1",
          "p": {
              "s": "EURUSD"
          },
          "h": {
              "X-YB-API-Key": "{{user.ApiKey}}",
              "X-YB-LOCALE": "en"
          },
          "reqId": "777816"
      }
      """;
await WebSockerHelpers.SendAsync(socket, unsubscribeMessageJson);
await Task.Delay(1000);

// Close connection
await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
Console.WriteLine("🔌 WebSocket closed.");

await listenTask;

return;

async Task<ApiToken?> Authorise(int login, AuthenticationMethod authenticationMethod)
{
    var payload = new AuthReqeust { Login = login };

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

async Task AddSymbol(Symbol symbol, AuthenticationMethod authenticationMethod)
{
    using var client = new HttpClient();
    var headers = ApiHeaders.GetPostHeaders(user, symbol, authenticationMethod);

    var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(tradeServerAdminApiUrl), "/api/v1/admin/symbols/edit"))
    {
        Content = new StringContent(JsonSerializer.Serialize(symbol, ApiHeaders.JsonSerializerOptions),
            Encoding.UTF8, "application/json")
    };
    
    foreach (var header in headers)
    {
        request.Headers.Add(header.Key, header.Value);
    }

    var response = await client.SendAsync(request);
    var responseContent = await response.Content.ReadAsStringAsync();

    Console.WriteLine("Response:");
    Console.WriteLine(responseContent);
}

async Task QuerySymbols(AuthenticationMethod authenticationMethod)
{
    using var client = new HttpClient();
    var headers = ApiHeaders.GetPostHeaders(user, "", authenticationMethod);

    var request = new HttpRequestMessage(HttpMethod.Get,
        new Uri(new Uri(tradeServerAdminApiUrl), "/api/v1/admin/symbols/query?maxResults=1000"));

    foreach (var header in headers)
    {
        request.Headers.Add(header.Key, header.Value);
    }

    var response = await client.SendAsync(request);
    var responseContent = await response.Content.ReadAsStringAsync();

    Console.WriteLine("Response:");
    Console.WriteLine(responseContent);
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