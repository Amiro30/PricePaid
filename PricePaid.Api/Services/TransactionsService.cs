namespace PricePaid.Api.Services;

using PricePaid.Api.Models;

public class TransactionsService : ITransactionsService
{
    private readonly ICsvReaderService _csvReader;

    public TransactionsService(ICsvReaderService csvReader)
    {
        _csvReader = csvReader;
    }

    public async Task<TransactionsPage> GetPageAsync(int pageSize, long byteOffset)
    {
        var lines = new List<string>(pageSize);
        var nextOffset = byteOffset;

        await foreach (var (transaction, offset) in _csvReader.ReadWithOffsetAsync(byteOffset))
        {
            lines.Add(transaction.ToCsvLine());
            nextOffset = offset;

            if (lines.Count >= pageSize)
                break;
        }

        return new TransactionsPage(lines, nextOffset);
    }
}
