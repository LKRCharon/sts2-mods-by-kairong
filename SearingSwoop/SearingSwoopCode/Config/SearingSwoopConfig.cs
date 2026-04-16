using BaseLib.Config;

namespace SearingSwoop.SearingSwoopCode.Config;

public sealed class SearingSwoopConfig : SimpleModConfig
{
    [ConfigSection("General")]
    [ConfigHoverTip]
    public static bool EnableModContent { get; set; } = true;
}
