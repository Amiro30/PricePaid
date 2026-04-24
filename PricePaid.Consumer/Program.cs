using System.Globalization;
using System.Text;
using PricePaid.Consumer;


const int pageSize = 100;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("Starting...");

using var client = new ApiClient("http://localhost:5179");
var aggregator = new TownPriceAggregator();

var totalPages = 0;
var totalRecords = 0;
var outputPath = "results.txt";

await foreach (var page in client.GetPagesAsync(pageSize))
{
    totalPages++;

    foreach (var line in page.Lines)
    {
        var fields = CsvParser.ParseLine(line);

        if (fields.Length < 16)
            continue;

        var town = fields[11].Trim();

        if (string.IsNullOrWhiteSpace(town))
            continue;

        if (!decimal.TryParse(fields[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var price))
            continue;

        aggregator.Add(town, price);
        totalRecords++;
    }

    if (totalPages % 10 == 0)
        Console.WriteLine($"Pages: {totalPages}, Records: {totalRecords}");
}

await using var writer = new StreamWriter(outputPath, append: false, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

void WriteLine(string line = "")
{
    Console.WriteLine(line);
    writer.WriteLine(line);
}

WriteLine();
WriteLine($"Page size: {pageSize}");
WriteLine($"Total pages: {totalPages}");
WriteLine($"Total records processed: {totalRecords}");
WriteLine();
WriteLine("Average price by town (sorted):");

foreach (var result in aggregator.GetSortedByTown())
{
    WriteLine($"{result.Town}: £{result.AveragePrice.ToString("N0", CultureInfo.InvariantCulture)} ({result.Count} transactions)");
}

await writer.FlushAsync();
Console.ReadKey();