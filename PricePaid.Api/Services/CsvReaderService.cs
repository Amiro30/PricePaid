namespace PricePaid.Api.Services;

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using PricePaid.Api.Models;

public class CsvReaderService : ICsvReaderService
{
    private const int BufferSize = 65536;

    private readonly string _filePath;

    public CsvReaderService(string filePath)
    {
        _filePath = filePath;
    }

    public async IAsyncEnumerable<(Transaction Transaction, long NextOffset)> ReadWithOffsetAsync(
        long byteOffset,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var stream = new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: BufferSize,
            useAsync: true);

        if (byteOffset >= stream.Length)
            yield break;

        stream.Seek(byteOffset, SeekOrigin.Begin);

        await foreach (var (line, endOffset) in ReadLinesAsync(stream, byteOffset, cancellationToken))
        {
            var transaction = ParseLine(line);

            if (transaction is not null)
                yield return (transaction, endOffset);
        }
    }

    private static async IAsyncEnumerable<(string Line, long EndOffset)> ReadLinesAsync(
        Stream stream,
        long startOffset,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = new byte[BufferSize];
        using var lineBuffer = new MemoryStream(capacity: 512);

        var position = startOffset;

        while (true)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);

            if (bytesRead == 0)
            {
                if (lineBuffer.Length > 0)
                {
                    var line = DecodeLine(lineBuffer);
                    yield return (line, position);
                }

                yield break;
            }

            for (var i = 0; i < bytesRead; i++)
            {
                var b = buffer[i];
                position++;

                if (b == '\n')
                {
                    var line = DecodeLine(lineBuffer);
                    yield return (line, position);

                    lineBuffer.SetLength(0);
                }
                else
                {
                    lineBuffer.WriteByte(b);
                }
            }
        }
    }

    private static string DecodeLine(MemoryStream lineBuffer)
    {
        var bytes = lineBuffer.GetBuffer();
        var length = (int)lineBuffer.Length;

        if (length > 0 && bytes[length - 1] == '\r')
            length--;

        return Encoding.UTF8.GetString(bytes, 0, length);
    }

    private static Transaction? ParseLine(string line)
    {
        var fields = ParseCsvLine(line);

        if (fields.Length < 16)
            return null;

        if (!decimal.TryParse(fields[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            return null;

        if (!DateTime.TryParse(fields[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return null;

        return new Transaction(
            TransactionId: fields[0],
            Price: price,
            DateOfTransfer: date,
            Postcode: fields[3],
            PropertyType: fields[4],
            OldNew: fields[5],
            Duration: fields[6],
            PAON: fields[7],
            SAON: fields[8],
            Street: fields[9],
            Locality: fields[10],
            TownCity: fields[11],
            District: fields[12],
            County: fields[13],
            PPDCategoryType: fields[14],
            RecordStatus: fields[15]
        );
    }

    private static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>(16);
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        fields.Add(current.ToString());

        return fields.ToArray();
    }
}