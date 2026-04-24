namespace PricePaid.Consumer;

public sealed class TownPriceAggregator
{
    private readonly Dictionary<string, TownPriceStats> _data =
        new(StringComparer.OrdinalIgnoreCase);

    public void Add(string town, decimal price)
    {
        if (string.IsNullOrWhiteSpace(town))
            return;

        town = town.Trim();

        if (!_data.TryGetValue(town, out var stats))
        {
            stats = new TownPriceStats();
            _data[town] = stats;
        }

        stats.TotalPrice += price;
        stats.Count++;
    }

    public IEnumerable<TownAverage> GetSortedByTown()
    {
        return _data
            .OrderBy(x => x.Key)
            .Select(x => new TownAverage(
                Town: x.Key,
                AveragePrice: x.Value.TotalPrice / x.Value.Count,
                Count: x.Value.Count));
    }

    private sealed class TownPriceStats
    {
        public decimal TotalPrice { get; set; }
        public int Count { get; set; }
    }
}

public sealed record TownAverage(string Town, decimal AveragePrice, int Count);