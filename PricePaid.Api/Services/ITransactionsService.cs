namespace PricePaid.Api.Services;

public record TransactionsPage(IReadOnlyList<string> Lines, long NextOffset);

public interface ITransactionsService
{
    Task<TransactionsPage> GetPageAsync(int pageSize, long byteOffset);
}