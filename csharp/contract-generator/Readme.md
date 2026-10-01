# C# Contract Generator

This tool turns the Trade Server API specs into plain C# classes that you copy into your own project. You use them with your own `HttpClient` code and System.Text.Json, in place of request and response classes written by hand.

For contracts that update themselves on every build, together with a ready-made client, see [generated-api-client](../generated-api-client) instead.

## Requirements

- .NET 10 SDK, to run the generator.
- In your project: .NET 9 or later, or .NET 8 with the `System.Text.Json` package at version 9.0 or later.

## Generate the Contracts

1. Download the two specs from the docs site, with the docs site login, into the `specs` folder:

   | Download | Save as |
   | ----- | ----- |
   | https://docs.yourbourse.trade/api/openapi.yaml | `specs/public_api.yaml` |
   | https://docs.yourbourse.trade/admin-api/openapi.yaml | `specs/admin_api.yaml` |

   Both files are called `openapi.yaml` on the site, so rename them as you save them. The file name decides the name of the output file and its namespace.

2. Run the generator:
   ```bash
   cd csharp/contract-generator
   dotnet run
   ```

3. Copy `Contracts/PublicApi.cs` and `Contracts/AdminApi.cs` into your project.

The classes go into the `TradeServer.Contracts.PublicApi` and `TradeServer.Contracts.AdminApi` namespaces. To use your own namespace instead, pass it in: `dotnet run -- MyCompany.TradeServer`.

When the API changes, download the specs again, run the generator again and replace the copied files. Neither `specs` nor `Contracts` is committed to this repo.

## Using the Classes

- **Pass `JsonSerializerOptions.Default` to `HttpClient`'s JSON helpers**, such as `GetFromJsonAsync`, `ReadFromJsonAsync` and `PostAsJsonAsync`:
  ```csharp
  var order = await http.GetFromJsonAsync<Order>(url, JsonSerializerOptions.Default);
  ```
  Without it, these helpers match JSON names regardless of case. Some classes have two JSON names that differ only by case, so the helpers throw `InvalidOperationException: The JSON property name ... collides with another property`.
- **Property names follow the JSON names**, so `b` becomes `B`. Hover over a property in your IDE to see its description. When two JSON names differ only by case, the one with the capital letter gets an `Upper` suffix: `order.S` is the symbol and `order.SUpper` is the side.
- **Optional fields left as `null` are not sent**, and enum values are sent exactly as the API spells them, for example `buy` rather than `Buy`.
- **A few admin fields can hold one of several shapes**, such as the actions of an order routing rule. The generator lists them when it runs, and they are typed `object`. When reading, you get a `JsonElement`: check its `t` field and convert it to the matching class.
  ```csharp
  var action = (JsonElement)rule.A.First();
  if (action.GetProperty("t").GetString() == "Delay")
  {
      var delay = action.Deserialize<OrderRoutingActionDelay>();
  }
  ```
  When writing, assign the matching class: `rule.A = [new OrderRoutingActionDelay { T = OrderRoutingActionDelayT.Delay, Mind = 10, Maxd = 20 }]`.
- **The classes only describe the data.** Sending requests and signing them stays in your code. See [api-example](../api-example) for how to sign requests.
