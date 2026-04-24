using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PricePaid.Tests;

[TestFixture]
public class TransactionsApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetTransactions_FirstPage_ReturnsCsvAndNextToken()
    {
        var response = await _client.GetAsync("/transactions?pageSize=5");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/csv"));
        Assert.That(response.Headers.Contains("X-Next-Page-Token"), Is.True);

        var csv = await response.Content.ReadAsStringAsync();
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.That(lines.Length, Is.EqualTo(5));
    }

    [Test]
    public async Task GetTransactions_SecondPage_DoesNotDuplicateFirstPage()
    {
        var firstResponse = await _client.GetAsync("/transactions?pageSize=5");
        firstResponse.EnsureSuccessStatusCode();

        var firstToken = firstResponse.Headers.GetValues("X-Next-Page-Token").Single();

        var secondResponse = await _client.GetAsync(
            $"/transactions?pageSize=5&pageToken={Uri.EscapeDataString(firstToken)}");

        Assert.That(secondResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var firstLines = (await firstResponse.Content.ReadAsStringAsync())
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var secondLines = (await secondResponse.Content.ReadAsStringAsync())
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Assert.That(firstLines.Intersect(secondLines), Is.Empty);
    }

    [Test]
    public async Task GetTransactions_InvalidPageToken_ReturnsBadRequest()
    {
        var response = await _client.GetAsync("/transactions?pageSize=5&pageToken=INVALID_TOKEN");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetTransactions_EndOfDataToken_ReturnsNoContent()
    {
        var endToken = Convert.ToBase64String(BitConverter.GetBytes(long.MaxValue));

        var response = await _client.GetAsync(
            $"/transactions?pageSize=5&pageToken={Uri.EscapeDataString(endToken)}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task GetTransactions_SchemaQuery_ReturnsJsonSchema()
    {
        var response = await _client.GetAsync("/transactions?schema");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));

        var json = await response.Content.ReadAsStringAsync();

        Assert.That(json, Does.Contain("TransactionId"));
        Assert.That(json, Does.Contain("Price"));
        Assert.That(json, Does.Contain("TownCity"));
    }
}