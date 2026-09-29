using System.Net;
using KiotaExample;
using KiotaExample.AdminApi;
using KiotaExample.PublicApi;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

const string publicApiUrl = "https://yourbourse.trade:2xxxx";
const long login = 1;
const string password = "xxx";

const string adminApiUrl = "https://yourbourse.trade:2xxxx";
const long managerLogin = 1;
const string managerPassword = "xxx";

// Kiota error exceptions have no message of their own, so print what the server sent back.
try
{
    Console.WriteLine("================================== Public API: authorize ==================================");
    var session = new TradeServerSession(password);
    var publicApi = new PublicApiClient(CreateRequestAdapter(publicApiUrl, session));

    var token = await publicApi.Authorize.PostAsync(new() { Login = login });
    session.SignIn(token!.Token!, token.SigningToken!);
    Console.WriteLine($"Signed in to trading account {token.Account}");

    Console.WriteLine("================================== Public API: account state ==================================");
    var state = await publicApi.Account.State.PostAsync();
    Console.WriteLine($"Balance: {state!.B} {state.C}, equity: {state.E}");

    Console.WriteLine("================================== Public API: all symbols, page by page ==================================");
    string? nextToken = null;
    do
    {
        var page = await publicApi.Symbols.Query.GetAsync(config =>
        {
            config.QueryParameters.MaxResults = 100;
            if (nextToken is not null)
            {
                config.Headers.Add("X-YB-NEXT-TOKEN", nextToken);
            }
        });

        foreach (var symbol in page!.Symbols!)
        {
            Console.WriteLine($"{symbol.N}: {symbol.D}");
        }

        nextToken = page.NextToken;
    } while (nextToken is not null);

    Console.WriteLine("================================== Public API: error handling ==================================");
    try
    {
        await publicApi.Symbols.GetPath["NO-SUCH-SYMBOL"].GetAsync();
    }
    catch (KiotaExample.PublicApi.Models.ProblemDetails problem)
    {
        Console.WriteLine($"HTTP {problem.ResponseStatusCode}: {problem.Title} {problem.Detail}");
    }

    Console.WriteLine("================================== Admin API: authorize ==================================");
    var adminSession = new TradeServerSession(managerPassword);
    var adminApi = new AdminApiClient(CreateRequestAdapter(adminApiUrl, adminSession));

    var adminToken = await adminApi.Authorize.PostAsync(new() { Login = managerLogin });
    adminSession.SignIn(adminToken!.Token!, adminToken.SigningToken!);

    Console.WriteLine("================================== Admin API: groups ==================================");
    var groups = await adminApi.Admin.Groups.Query.GetAsync();
    foreach (var group in groups!.Groups!)
    {
        Console.WriteLine($"{group.Id}: {group.Name}");
    }

    await new HolidayExample(adminApi).RunAsync();
}
catch (KiotaExample.PublicApi.Models.ProblemDetails problem)
{
    Console.WriteLine($"Public API error, HTTP {problem.ResponseStatusCode}: {problem.Title} {problem.Detail} [{problem.Code}]");
}
catch (KiotaExample.AdminApi.Models.ProblemDetails problem)
{
    Console.WriteLine($"Admin API error, HTTP {problem.ResponseStatusCode}: {problem.Title} {problem.Detail} [{problem.Code}]");
}

return;

static HttpClientRequestAdapter CreateRequestAdapter(string tradeServerUrl, TradeServerSession session)
{
    // Kiota also retries 503 and 504 by default. The server may already have acted on those,
    // so a retried order could be placed twice. A 429 means the request was not processed.
    var retryOnlyWhenRateLimited = new RetryHandlerOption
    {
        ShouldRetry = (_, _, response) => response.StatusCode == HttpStatusCode.TooManyRequests
    };

    var handlers = KiotaClientFactory.CreateDefaultHandlers([retryOnlyWhenRateLimited]);

    // Last in the chain, so every retry is signed again.
    handlers.Add(new SigningHandler(session));

    return new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: KiotaClientFactory.Create(handlers))
    {
        BaseUrl = $"{tradeServerUrl}/api/v1"
    };
}
