using System.Net;

namespace PricePaid.Consumer;

public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl.TrimEnd('/');
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async IAsyncEnumerable<ApiPage> GetPagesAsync(int pageSize)
    {
        await Task.Delay(3000);
        string? pageToken = null;

        while (true)
        {
            var url = $"{_baseUrl}/transactions?pageSize={pageSize}";

            if (!string.IsNullOrWhiteSpace(pageToken))
                url += $"&pageToken={Uri.EscapeDataString(pageToken)}";

            using var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.NoContent)
                yield break;

            response.EnsureSuccessStatusCode();

            var csv = await response.Content.ReadAsStringAsync();

            var nextToken = response.Headers.TryGetValues("X-Next-Page-Token", out var values)
                ? values.FirstOrDefault()
                : throw new InvalidOperationException("Missing X-Next-Page-Token header.");

            var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            yield return new ApiPage(lines, nextToken);

            pageToken = nextToken;
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

public sealed record ApiPage(IReadOnlyList<string> Lines, string NextToken);