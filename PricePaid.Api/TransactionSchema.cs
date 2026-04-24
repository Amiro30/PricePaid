namespace PricePaid.Api;

public static class TransactionSchema
{
    public static readonly object[] Items =
    [
        new { Index = 0,  Name = "TransactionId",   Type = "string" },
        new { Index = 1,  Name = "Price",           Type = "decimal" },
        new { Index = 2,  Name = "DateOfTransfer",  Type = "datetime" },
        new { Index = 3,  Name = "Postcode",        Type = "string" },
        new { Index = 4,  Name = "PropertyType",    Type = "string" },
        new { Index = 5,  Name = "OldNew",          Type = "string" },
        new { Index = 6,  Name = "Duration",        Type = "string" },
        new { Index = 7,  Name = "PAON",            Type = "string" },
        new { Index = 8,  Name = "SAON",            Type = "string" },
        new { Index = 9,  Name = "Street",          Type = "string" },
        new { Index = 10, Name = "Locality",        Type = "string" },
        new { Index = 11, Name = "TownCity",        Type = "string" },
        new { Index = 12, Name = "District",        Type = "string" },
        new { Index = 13, Name = "County",          Type = "string" },
        new { Index = 14, Name = "PPDCategoryType", Type = "string" },
        new { Index = 15, Name = "RecordStatus",    Type = "string" }
    ];
}