using System.Net.Http.Headers;
using System.Net.Http.Json;
using expense_tracker.Models;
using expense_tracker.Models.Akahu;
using Microsoft.Extensions.Options;

namespace expense_tracker.Services.Akahu;

// Typed HttpClient for the Akahu Personal App API. Registered via AddHttpClient<AkahuClient>().
public class AkahuClient
{
    private readonly HttpClient _http;

    public AkahuClient(HttpClient http, IOptions<AkahuOptions> options)
    {
        var opts = options.Value;
        _http = http;
        _http.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
        _http.DefaultRequestHeaders.Add("X-Akahu-Id", opts.AppToken);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opts.UserToken);
    }

    public async Task<AkahuMe> GetMeAsync(CancellationToken ct = default)
    {
        var response = await _http.GetFromJsonAsync<AkahuItemResponse<AkahuMe>>("me", ct)
            ?? throw new InvalidOperationException("Empty response from Akahu (me).");
        return response.Item;
    }

    public async Task<List<AkahuAccount>> GetAccountsAsync(CancellationToken ct = default)
    {
        var response = await _http.GetFromJsonAsync<AkahuListResponse<AkahuAccount>>("accounts", ct)
            ?? throw new InvalidOperationException("Empty response from Akahu (accounts).");
        return response.Items;
    }

    // Follows cursor.next until null and returns every page concatenated.
    public async Task<List<AkahuTransaction>> GetAccountTransactionsAsync(
        string accountId, DateTime? start, DateTime? end, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (start.HasValue) query.Add("start=" + Uri.EscapeDataString(start.Value.ToString("o")));
        if (end.HasValue) query.Add("end=" + Uri.EscapeDataString(end.Value.ToString("o")));

        var basePath = $"accounts/{Uri.EscapeDataString(accountId)}/transactions";
        var all = new List<AkahuTransaction>();
        string? cursor = null;

        do
        {
            var parts = new List<string>(query);
            if (cursor is not null) parts.Add("cursor=" + Uri.EscapeDataString(cursor));
            var url = parts.Count > 0 ? basePath + "?" + string.Join("&", parts) : basePath;

            var page = await _http.GetFromJsonAsync<AkahuListResponse<AkahuTransaction>>(url, ct)
                ?? throw new InvalidOperationException("Empty response from Akahu (transactions).");

            all.AddRange(page.Items);
            cursor = page.Cursor?.Next;
        } while (!string.IsNullOrEmpty(cursor));

        return all;
    }
}
