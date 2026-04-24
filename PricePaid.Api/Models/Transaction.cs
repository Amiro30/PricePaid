namespace PricePaid.Api.Models;

public record Transaction(
    string TransactionId,
    decimal Price,
    DateTime DateOfTransfer,
    string Postcode,
    string PropertyType,
    string OldNew,
    string Duration,
    string PAON,
    string SAON,
    string Street,
    string Locality,
    string TownCity,
    string District,
    string County,
    string PPDCategoryType,
    string RecordStatus
)
{
    public string ToCsvLine() =>
        $"\"{TransactionId}\",\"{Price}\",\"{DateOfTransfer:yyyy-MM-dd HH:mm}\"," +
        $"\"{Postcode}\",\"{PropertyType}\",\"{OldNew}\",\"{Duration}\"," +
        $"\"{PAON}\",\"{SAON}\",\"{Street}\",\"{Locality}\"," +
        $"\"{TownCity}\",\"{District}\",\"{County}\"," +
        $"\"{PPDCategoryType}\",\"{RecordStatus}\"";
}