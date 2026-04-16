using System.Reflection;
using Godot;

namespace SearingSwoop.SearingSwoopCode.Utils;

internal static class CardPortraitLoader
{
    private static readonly Dictionary<string, Texture2D> Cache = [];
    private static readonly HashSet<string> LoggedOnce = [];
    private static readonly string? AssemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    private static readonly string? NestedModDir = string.IsNullOrWhiteSpace(AssemblyDir)
        ? null
        : Path.Combine(AssemblyDir, MainFile.ModId);

    public static Texture2D? LoadPortrait(string portraitFileName)
    {
        // Prefer mod art and fall back to default card portrait.
        foreach (string relativePath in CandidateRelativePaths(portraitFileName))
        {
            Texture2D? loaded = TryLoadFromDisk(relativePath);
            if (loaded != null)
            {
                return loaded;
            }
        }

        LogOnce($"fallback:{portraitFileName}", $"Portrait fallback to vanilla for '{portraitFileName}'.");
        return ResourceLoader.Load<Texture2D>("res://images/card_portraits/card.png");
    }

    private static IEnumerable<string> CandidateRelativePaths(string portraitFileName)
    {
        yield return Path.Combine("images", "card_portraits", portraitFileName);
        yield return Path.Combine("images", "card_portraits", "big", portraitFileName);
        yield return Path.Combine("images", "card_portraits", "card.png");
        yield return Path.Combine("images", "card_portraits", "big", "card.png");
    }

    private static Texture2D? TryLoadFromDisk(string relativePath)
    {
        foreach (string absolutePath in CandidateAbsolutePaths(relativePath))
        {
            string normalized = absolutePath.Replace('\\', '/');

            if (Cache.TryGetValue(normalized, out Texture2D? cached))
            {
                return cached;
            }

            if (!File.Exists(absolutePath))
            {
                LogOnce($"miss:{normalized}", $"Portrait file missing: {absolutePath}");
                continue;
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(absolutePath);
                Image image = new();
                Error err = image.LoadPngFromBuffer(bytes);
                if (err != Error.Ok)
                {
                    // Fallback decoder for non-png image files.
                    err = image.Load(absolutePath);
                }

                if (err != Error.Ok)
                {
                    MainFile.Logger.Warn($"Portrait decode failed: {absolutePath}, error={err}");
                    continue;
                }

                ImageTexture texture = ImageTexture.CreateFromImage(image);
                texture.TakeOverPath("res://images/card_portraits/card.png");
                Cache[normalized] = texture;
                LogOnce($"ok:{normalized}", $"Portrait loaded from disk: {absolutePath} ({image.GetWidth()}x{image.GetHeight()})");
                return texture;
            }
            catch (Exception ex)
            {
                MainFile.Logger.Warn($"Portrait load exception for '{absolutePath}': {ex}");
            }
        }

        return null;
    }

    private static IEnumerable<string> CandidateAbsolutePaths(string relativePath)
    {
        if (!string.IsNullOrWhiteSpace(NestedModDir))
        {
            yield return Path.Combine(NestedModDir, relativePath);
        }

        if (!string.IsNullOrWhiteSpace(AssemblyDir))
        {
            yield return Path.Combine(AssemblyDir, relativePath);
        }
    }

    private static void LogOnce(string key, string message)
    {
        if (LoggedOnce.Add(key))
        {
            MainFile.Logger.Info(message);
        }
    }
}
