using GeneratedApiClient.AdminApi;
using GeneratedApiClient.AdminApi.Admin.Holidays;
using GeneratedApiClient.AdminApi.Models;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

namespace GeneratedApiClient;

/// <summary>
/// Creates, reads, edits and deletes holidays through the admin API, first one at a time, then in a batch.
/// Everything it creates is deleted again.
/// </summary>
public class HolidayExample(AdminApiClient adminApi)
{
    private readonly HolidaysRequestBuilder holidays = adminApi.Admin.Holidays;

    public async Task RunAsync()
    {
        await RunOneAtATimeAsync();
        await RunBatchAsync();
    }

    private async Task RunOneAtATimeAsync()
    {
        Console.WriteLine("================================== Admin API: create a holiday ==================================");
        var created = await holidays.Edit.PostAsync(NewHoliday("Generated API client example", day: 31));
        long id = created!.Id!.Value;
        Console.WriteLine($"Created holiday {id}, version {created.Version}");

        try
        {
            Console.WriteLine("================================== Admin API: read it ==================================");
            var (holiday, etag) = await GetAsync(id);
            Console.WriteLine($"{holiday.Description}, ETag {etag}");

            Console.WriteLine("================================== Admin API: edit it ==================================");
            holiday.Description = "Generated API client example, edited";
            await holidays.Edit.PostAsync(holiday, config => config.Headers.Add("If-Match", etag));

            (holiday, etag) = await GetAsync(id);
            Console.WriteLine($"{holiday.Description}, ETag {etag}");

            Console.WriteLine("================================== Admin API: find it in the list ==================================");
            var page = await holidays.Query.PostAsync(new() { MaxResults = 100 });
            var found = page!.Holidays!.Any(h => h.Id == id);
            Console.WriteLine(found ? $"Holiday {id} is on the first page" : $"Holiday {id} is not on the first page");
        }
        finally
        {
            Console.WriteLine("================================== Admin API: delete it ==================================");
            await DeleteAsync(id);
        }

        try
        {
            await holidays.GetPath[id].GetAsync();
            Console.WriteLine($"Holiday {id} is still there");
        }
        // Code 1 means not found. This endpoint sends it with HTTP 400, although the spec says 404.
        catch (ProblemDetails problem) when (problem.Code == "1")
        {
            Console.WriteLine($"Holiday {id} is gone");
        }
    }

    private async Task RunBatchAsync()
    {
        Console.WriteLine("================================== Admin API: create two holidays in one call ==================================");
        var addResults = await holidays.Batch.Edit.PostAsync([NewHoliday("Generated API client batch example 1", day: 29), NewHoliday("Generated API client batch example 2", day: 30)]) ?? [];

        // Each item succeeds or fails on its own, even when the call as a whole succeeds.
        foreach (var result in addResults.Where(r => r.S != true))
        {
            Console.WriteLine($"Not created: {result.D} [{result.C}]");
        }

        var created = addResults.Where(r => r.S == true).Select(r => r.H!).ToList();
        Console.WriteLine($"Created holidays {string.Join(", ", created.Select(h => h.Id))}");
        if (created.Count == 0)
        {
            return;
        }

        Console.WriteLine("================================== Admin API: delete both in one call ==================================");
        var deleteResults = await holidays.Batch.DeletePath.PostAsync(created.Select(h => new HolidayToDelete { HolidayId = h.Id, Version = h.Version }).ToList()) ?? [];
        foreach (var result in deleteResults)
        {
            Console.WriteLine(result.S == true ? $"Deleted holiday {result.Id}" : $"Holiday {result.Id} not deleted: {result.D} [{result.C}]");
        }
    }

    // The ETag response header holds the version that edits and deletes must send back in If-Match.
    private async Task<(Holiday Holiday, string ETag)> GetAsync(long id)
    {
        var headers = new HeadersInspectionHandlerOption { InspectResponseHeaders = true };
        var holiday = await holidays.GetPath[id].GetAsync(config => config.Options.Add(headers));

        return (holiday!, headers.ResponseHeaders["ETag"].Single());
    }

    private async Task DeleteAsync(long id)
    {
        var (holiday, etag) = await GetAsync(id);
        await holidays.DeletePath.PostAsync(new() { HolidayId = id, Version = holiday.Version }, config => config.Headers.Add("If-Match", etag));
        Console.WriteLine($"Deleted holiday {id}");
    }

    // Disabled, far in the future and matching no symbol, so it can never close trading.
    private static Holiday NewHoliday(string description, int day) => new()
    {
        Id = 0,
        Version = 0,
        Description = description,
        Enabled = false,
        Year = 2099,
        Month = 12,
        Day = day,
        SymbolMask = "GENERATED-API-CLIENT-EXAMPLE"
    };
}
