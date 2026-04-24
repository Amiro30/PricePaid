namespace PricePaid.Api;

internal static class PageToken
{
    internal static string Encode(long offset)
        => Convert.ToBase64String(BitConverter.GetBytes(offset));

    internal static bool TryDecode(string token, out long offset)
    {
        offset = 0;
        try
        {
            var bytes = Convert.FromBase64String(token);
            if (bytes.Length != 8) return false;
            offset = BitConverter.ToInt64(bytes);
            return offset >= 0;
        }
        catch
        {
            return false;
        }
    }
}
