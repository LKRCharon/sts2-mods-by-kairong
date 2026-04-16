using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using SearingSwoop.SearingSwoopCode.Config;

namespace SearingSwoop.SearingSwoopCode;

// You're recommended but not required to keep all your code in this package and all your assets in the mod folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "SearingSwoop";
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);
    internal static bool IsModContentEnabled => SearingSwoopConfig.EnableModContent;

    public static void Initialize()
    {
        Logger.Info("Registering Searing Swoop mod config.");
        ModConfigRegistry.Register(ModId, new SearingSwoopConfig());
        Logger.Info("Mod config registered.");
        Harmony harmony = new(ModId);

        harmony.PatchAll();
    }
}
