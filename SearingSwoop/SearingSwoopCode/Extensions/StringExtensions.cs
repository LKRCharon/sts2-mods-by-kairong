namespace SearingSwoop.SearingSwoopCode.Extensions;

internal static class StringExtensions
{
    public static string CardImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "card_portraits", path);
    }

    public static string BigCardImagePath(this string path)
    {
        return Path.Join(MainFile.ResPath, "images", "card_portraits", "big", path);
    }
}
