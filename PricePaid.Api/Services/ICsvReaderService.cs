using PricePaid.Api.Models;

namespace PricePaid.Api.Services
{
    public interface ICsvReaderService
    {
        IAsyncEnumerable<(Transaction Transaction, long NextOffset)> ReadWithOffsetAsync(long byteOffset, CancellationToken cancellationToken = default);
    }
}
