using Microsoft.AspNetCore.Mvc;
using PricePaid.Api.Services;

namespace PricePaid.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionsController : ControllerBase
{
    private const int MaxPageSize = 1000; // prevents excessive memory usage
    private readonly ITransactionsService _transactions;

    public TransactionsController(ITransactionsService transactions)
    {
        _transactions = transactions;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? pageSize, [FromQuery] string? pageToken = null)
    {
        var byteOffset = 0L;

        if (Request.Query.ContainsKey("schema"))
            return Ok(TransactionSchema.Items);

        if (pageSize is null || pageSize <= 0)
            return BadRequest("pageSize must be greater than 0");

        if (pageSize > MaxPageSize)
            return BadRequest($"pageSize must not exceed {MaxPageSize}");
        
        if (!string.IsNullOrWhiteSpace(pageToken) &&
            !PageToken.TryDecode(pageToken, out byteOffset))
        {
            return BadRequest("Invalid pageToken");
        }

        var page = await _transactions.GetPageAsync(pageSize.Value, byteOffset);

        if (page.Lines.Count == 0)
            return NoContent();

        Response.Headers["X-Next-Page-Token"] = PageToken.Encode(page.NextOffset);

        return Content(string.Join('\n', page.Lines), "text/csv");
    }
}