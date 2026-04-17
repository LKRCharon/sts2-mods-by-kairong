using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using SearingSwoop.SearingSwoopCode.Cards;
using MonsterByrdpip = MegaCrit.Sts2.Core.Models.Monsters.Byrdpip;
using RelicByrdpip = MegaCrit.Sts2.Core.Models.Relics.Byrdpip;

namespace SearingSwoop.SearingSwoopCode;

internal static class SearingSwoopState
{
    internal static readonly SavedSpireField<RelicByrdpip, int> HatchCount =
        new(() => 0, "SearingSwoop_HatchCount");

    internal static readonly SavedSpireField<RelicByrdpip, int[]> HatchSkinIndices =
        new(() => Array.Empty<int>(), "SearingSwoop_HatchSkinIndices");

    private const string ByrdpipPluralRelicTitleLocKey = "SEARING_SWOOP_BYRDPIP_PLURAL.title";
    private const string EggDescriptionLocKey = "SEARING_SWOOP_EGG.description";
    private const string SwoopDescriptionLocKey = "SEARING_SWOOP_SWOOP.description";
    private const string ByrdpipDescriptionLocKey = "SEARING_SWOOP_BYRDPIP.description";

    private static readonly SpireField<Player, Queue<string>> PendingBirdSkins =
        new(_ => new Queue<string>());

    private static readonly SpireField<Creature, string?> AssignedBirdSkins =
        new(() => null);

    private static int _allowInternalSwoopUpgrades;
    private static int _allowSwoopDeserializeUpgrades;
    private static string? _registeredPluralRelicTitleLanguage;

    private sealed class Scope(Action onDispose) : IDisposable
    {
        private Action? _onDispose = onDispose;

        public void Dispose()
        {
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }

    internal static int GetHatchCount(Player? player)
    {
        if (player == null)
        {
            return 0;
        }

        RelicByrdpip? relic = player.GetRelic<RelicByrdpip>();
        return relic == null ? 0 : HatchCount[relic];
    }

    internal static void IncrementHatchCount(RelicByrdpip relic)
    {
        HatchCount[relic] = HatchCount[relic] + 1;
        EnsureSkinHistoryMatchesHatchCount(relic);
        MainFile.Logger.Info($"Incremented hatch count for relic {relic.Id.Entry} to {HatchCount[relic]}.");
    }

    internal static bool HasSwoopCard(Player player)
    {
        return GetSwoopCards(player).Count > 0;
    }

    internal static IReadOnlyList<Creature> GetBirdPets(Player? player)
    {
        if (player?.PlayerCombatState == null)
        {
            return [];
        }

        IReadOnlyList<Creature> pets = player.PlayerCombatState!.Pets;
        return pets
            .Where(pet => pet.Monster is MonsterByrdpip)
            .ToList();
    }

    internal static IReadOnlyList<SearingSwoopCard> GetSwoopCards(Player? player)
    {
        if (player == null)
        {
            return [];
        }

        IEnumerable<CardModel> deckCards = PileType.Deck.GetPile(player)!.Cards;
        IEnumerable<CardModel> combatCards = CombatManager.Instance?.IsInProgress == true
            ? player.PlayerCombatState?.AllCards ?? []
            : [];

        return deckCards
            .Concat(combatCards)
            .OfType<SearingSwoopCard>()
            .Distinct()
            .ToList();
    }

    internal static IReadOnlyList<SearingEggCard> GetEggCards(Player? player)
    {
        if (player == null)
        {
            return [];
        }

        IEnumerable<CardModel> deckCards = PileType.Deck.GetPile(player)!.Cards;
        IEnumerable<CardModel> combatCards = CombatManager.Instance?.IsInProgress == true
            ? player.PlayerCombatState?.AllCards ?? []
            : [];

        return deckCards
            .Concat(combatCards)
            .OfType<SearingEggCard>()
            .Distinct()
            .ToList();
    }

    internal static bool HasEggCard(Player? player) => GetEggCards(player).Count > 0;

    internal static bool RunAlreadyHasStartingEgg(MegaCrit.Sts2.Core.Runs.RunState runState)
    {
        return runState.Players.Any(HasEggCard);
    }

    internal static int GetExpectedBirdCount(Player? player)
    {
        if (player == null)
        {
            return 1;
        }

        int birdPets = CombatManager.Instance?.IsInProgress == true ? GetBirdPets(player).Count : 0;
        return Math.Max(1, Math.Max(GetHatchCount(player), birdPets));
    }

    internal static int GetSwoopDamageForUpgradeLevel(int upgradeLevel)
    {
        return 14;
    }

    internal static void SyncSwoopDamage(SearingSwoopCard card)
    {
        int expectedDamage = GetSwoopDamageForUpgradeLevel(card.CurrentUpgradeLevel);
        if (card.DynamicVars.Damage.BaseValue != expectedDamage)
        {
            MainFile.Logger.Info($"Syncing Searing Swoop damage from {card.DynamicVars.Damage.BaseValue} to {expectedDamage} at upgrade level {card.CurrentUpgradeLevel}.");
            card.DynamicVars.Damage.BaseValue = expectedDamage;
        }
    }

    internal static IDisposable AllowInternalSwoopUpgrade()
    {
        _allowInternalSwoopUpgrades++;
        MainFile.Logger.Info($"AllowInternalSwoopUpgrade entered. Depth: {_allowInternalSwoopUpgrades}.");
        return new Scope(() =>
        {
            _allowInternalSwoopUpgrades = Math.Max(0, _allowInternalSwoopUpgrades - 1);
            MainFile.Logger.Info($"AllowInternalSwoopUpgrade exited. Depth: {_allowInternalSwoopUpgrades}.");
        });
    }

    internal static bool CanUseInternalSwoopUpgrade() => _allowInternalSwoopUpgrades > 0;

    internal static void EnterSwoopDeserializeUpgradeScope()
    {
        _allowSwoopDeserializeUpgrades++;
        MainFile.Logger.Info($"EnterSwoopDeserializeUpgradeScope. Depth: {_allowSwoopDeserializeUpgrades}.");
    }

    internal static void ExitSwoopDeserializeUpgradeScope()
    {
        _allowSwoopDeserializeUpgrades = Math.Max(0, _allowSwoopDeserializeUpgrades - 1);
        MainFile.Logger.Info($"ExitSwoopDeserializeUpgradeScope. Depth: {_allowSwoopDeserializeUpgrades}.");
    }

    internal static bool CanUseDeserializeSwoopUpgrade() => _allowSwoopDeserializeUpgrades > 0;

    internal static bool CanBypassSwoopUpgradeLock() =>
        CanUseInternalSwoopUpgrade() || CanUseDeserializeSwoopUpgrade();

    internal static int GetLockedSwoopUpgradeLevel(SearingSwoopCard card)
    {
        try
        {
            // Searing Swoop level is determined by hatch count, not by generic upgrade mechanics.
            return Math.Max(0, GetHatchCount(card.Owner) - 1);
        }
        catch (Exception ex) when (IsCanonicalCardAccessException(ex))
        {
            return 0;
        }
    }

    internal static string DescribeBirdChainState(Player? player)
    {
        if (player == null)
        {
            return "player=null";
        }

        int hatchCount = GetHatchCount(player);
        int eggCount = GetEggCards(player).Count;
        int petCount = GetBirdPets(player).Count;
        string swoopLevels = string.Join(", ", GetSwoopCards(player)
            .Select((card, index) => $"#{index + 1}:+{card.CurrentUpgradeLevel}"));

        if (string.IsNullOrWhiteSpace(swoopLevels))
        {
            swoopLevels = "none";
        }

        return $"player={player.NetId}, hatchCount={hatchCount}, eggs={eggCount}, swoops={GetSwoopCards(player).Count} [{swoopLevels}], pets={petCount}, inCombat={CombatManager.Instance?.IsInProgress == true}";
    }

    internal static void RefreshRelicCounter(RelicByrdpip relic)
    {
        Traverse.Create(relic).Method("InvokeDisplayAmountChanged").GetValue();
    }

    internal static void EnsureCustomLocalization()
    {
        string? language = LocManager.Instance?.Language;
        if (LocManager.Instance == null || string.IsNullOrWhiteSpace(language) || language == _registeredPluralRelicTitleLanguage)
        {
            return;
        }

        LocTable locTable = LocManager.Instance.GetTable("relics");
        Dictionary<string, string> translations =
            AccessTools.Field(typeof(LocTable), "_translations").GetValue(locTable) as Dictionary<string, string>
            ?? throw new InvalidOperationException("Failed to access relic localization table.");

        translations[ByrdpipPluralRelicTitleLocKey] = IsChinese() ? "异鸟宝宝们" : "Byrdpips";
        _registeredPluralRelicTitleLanguage = language;
    }

    private static IReadOnlyList<string> GetSkinOptions()
    {
        string[] options = RelicByrdpip.SkinOptions
            .Where(option => !string.IsNullOrWhiteSpace(option))
            .Distinct()
            .ToArray();

        return options.Length > 0 ? options : ["default"];
    }

    private static int[] GetSavedSkinHistory(RelicByrdpip relic) => HatchSkinIndices[relic] ?? Array.Empty<int>();

    private static int NormalizeSkinIndex(int index, int optionCount)
    {
        if (optionCount <= 0)
        {
            return 0;
        }

        int normalized = index % optionCount;
        return normalized < 0 ? normalized + optionCount : normalized;
    }

    private static uint BuildSkinSeed(RelicByrdpip relic, int salt)
    {
        ulong seed = relic.Owner.RunState.Rng.Seed
            + relic.Owner.NetId * 397UL
            + (ulong)Math.Max(0, salt) * 7919UL;
        return unchecked((uint)seed);
    }

    private static int ChooseNextSkinIndex(RelicByrdpip relic, IReadOnlyList<string> skinOptions, IReadOnlyList<int> existingHistory)
    {
        if (skinOptions.Count <= 1)
        {
            return 0;
        }

        int[] allIndices = Enumerable.Range(0, skinOptions.Count).ToArray();
        HashSet<int> usedIndices = existingHistory
            .Select(index => NormalizeSkinIndex(index, skinOptions.Count))
            .ToHashSet();

        int[] candidates = allIndices
            .Where(index => !usedIndices.Contains(index))
            .ToArray();

        if (candidates.Length == 0)
        {
            int lastIndex = existingHistory.Count == 0
                ? -1
                : NormalizeSkinIndex(existingHistory[existingHistory.Count - 1], skinOptions.Count);

            candidates = allIndices
                .Where(index => index != lastIndex)
                .ToArray();
        }

        if (candidates.Length == 0)
        {
            candidates = allIndices;
        }

        return new Rng(BuildSkinSeed(relic, existingHistory.Count + 1)).NextItem(candidates);
    }

    internal static void EnsureSkinHistoryMatchesHatchCount(RelicByrdpip relic)
    {
        int requiredCount = Math.Max(0, HatchCount[relic]);
        List<int> history = GetSavedSkinHistory(relic).ToList();
        IReadOnlyList<string> skinOptions = GetSkinOptions();

        while (history.Count < requiredCount)
        {
            history.Add(ChooseNextSkinIndex(relic, skinOptions, history));
        }

        if (history.Count != GetSavedSkinHistory(relic).Length)
        {
            HatchSkinIndices[relic] = history.ToArray();
            MainFile.Logger.Info($"Skin history expanded for player {relic.Owner.NetId} to [{string.Join(", ", history)}].");
        }
    }

    internal static IReadOnlyList<string> PreparePendingBirdSkins(RelicByrdpip relic, int birdCount)
    {
        EnsureSkinHistoryMatchesHatchCount(relic);

        int[] history = GetSavedSkinHistory(relic);
        IReadOnlyList<string> skinOptions = GetSkinOptions();
        List<string> plannedSkins = new(birdCount);

        for (int i = 0; i < birdCount; i++)
        {
            int historyIndex = i < history.Length ? history[i] : ChooseNextSkinIndex(relic, skinOptions, history);
            plannedSkins.Add(skinOptions[NormalizeSkinIndex(historyIndex, skinOptions.Count)]);
        }

        PendingBirdSkins[relic.Owner] = new Queue<string>(plannedSkins);
        MainFile.Logger.Info($"Prepared planned Byrdpip skins for player {relic.Owner.NetId}: [{string.Join(", ", plannedSkins)}].");
        return plannedSkins;
    }

    internal static string GetOrAssignBirdSkin(Creature creature, RelicByrdpip relic)
    {
        if (!string.IsNullOrWhiteSpace(AssignedBirdSkins[creature]))
        {
            return AssignedBirdSkins[creature]!;
        }

        Queue<string> pendingSkins = PendingBirdSkins[relic.Owner]!;
        string? plannedSkin = pendingSkins.Count > 0 ? pendingSkins.Dequeue() : null;
        string assignedSkin = string.IsNullOrWhiteSpace(plannedSkin) ? relic.Skin : plannedSkin;
        AssignedBirdSkins[creature] = assignedSkin;
        MainFile.Logger.Info($"Assigned Byrdpip skin '{assignedSkin}' to creature {creature.CombatId?.ToString() ?? "no-combat-id"} for player {relic.Owner.NetId}.");
        return assignedSkin;
    }

    internal static void RememberBirdSkinIfNeeded(Creature creature, string plannedSkin)
    {
        if (string.IsNullOrWhiteSpace(plannedSkin) || !string.IsNullOrWhiteSpace(AssignedBirdSkins[creature]))
        {
            return;
        }

        AssignedBirdSkins[creature] = plannedSkin;

        Player owner = creature.PetOwner ?? throw new InvalidOperationException("Byrd pet had no owner while assigning skin.");
        Queue<string> pendingSkins = PendingBirdSkins[owner]!;
        if (pendingSkins.Count > 0 && pendingSkins.Peek() == plannedSkin)
        {
            pendingSkins.Dequeue();
        }

        MainFile.Logger.Info($"Late-bound Byrdpip skin '{plannedSkin}' onto creature {creature.CombatId?.ToString() ?? "no-combat-id"}.");
    }

    internal static bool IsChinese()
    {
        string? language = LocManager.Instance?.Language;
        return language is "zhs" or "zh_CN" or "zh-CN";
    }

    internal static string EggTitle() => IsChinese() ? "灼热鸟蛋" : "Searing Egg";

    internal static LocString ByrdpipPluralRelicTitle()
    {
        EnsureCustomLocalization();
        return new LocString("relics", ByrdpipPluralRelicTitleLocKey);
    }

    internal static LocString EggDescriptionLocString()
    {
        EnsureCustomCardLocalization();
        return new LocString("cards", EggDescriptionLocKey);
    }

    internal static string EggDescription()
    {
        return IsChinese()
            ? "能在[gold]休息处[/gold]被[gold]多次[/gold]孵化。"
            : "Can be hatched [gold]multiple times[/gold] at a [gold]Rest Site[/gold].";
    }

    internal static string SwoopTitle(SearingSwoopCard? card)
    {
        int upgradeLevel = Math.Max(0, card?.CurrentUpgradeLevel ?? 0);
        string baseTitle = IsChinese() ? "灼热扑击" : "Searing Swoop";
        return upgradeLevel <= 0 ? baseTitle : $"{baseTitle}+{upgradeLevel}";
    }

    internal static string SwoopDescription(SearingSwoopCard card, bool previewNextUpgrade = false)
    {
        TrySyncSwoopDamage(card);
        int shownUpgradeLevel = Math.Max(0, card.CurrentUpgradeLevel + (previewNextUpgrade ? 1 : 0));
        string damage = GetSwoopDamageForUpgradeLevel(shownUpgradeLevel).ToString();
        int hits = TryGetExpectedBirdCount(card);

        return IsChinese()
            ? $"造成{damage}点伤害[orange]{hits}[/orange]次。"
            : $"Deal {damage} damage [orange]{hits}[/orange] times.";
    }

    private static void TrySyncSwoopDamage(SearingSwoopCard card)
    {
        try
        {
            SyncSwoopDamage(card);
        }
        catch (Exception ex) when (IsCanonicalCardAccessException(ex))
        {
            // Card library / compendium can ask for canonical card descriptions.
            // Those cards have no mutable owner context, so sync is intentionally skipped.
        }
    }

    private static int TryGetExpectedBirdCount(SearingSwoopCard card)
    {
        try
        {
            return GetExpectedBirdCount(card.Owner);
        }
        catch (Exception ex) when (IsCanonicalCardAccessException(ex))
        {
            return 1;
        }
    }

    private static bool IsCanonicalCardAccessException(Exception ex)
    {
        return ex.GetType().Name.Contains("CanonicalModelException", StringComparison.Ordinal);
    }

    internal static LocString SwoopDescriptionLocString(SearingSwoopCard card)
    {
        EnsureCustomCardLocalization(card);
        return new LocString("cards", SwoopDescriptionLocKey);
    }

    internal static string ByrdpipDescription()
    {
        return IsChinese()
            ? "每次孵化获得1只异鸟伙伴，并锻造灼热扑击。"
            : "Each hatch grants 1 Byrdpip companion and upgrades Searing Swoop.";
    }

    internal static LocString ByrdpipDescriptionLocString()
    {
        EnsureCustomRelicLocalization();
        return new LocString("relics", ByrdpipDescriptionLocKey);
    }

    internal static void EnsureCustomCardLocalization(SearingSwoopCard? swoop = null)
    {
        if (LocManager.Instance == null)
        {
            return;
        }

        LocTable cardsTable = LocManager.Instance.GetTable("cards");
        Dictionary<string, string> translations =
            AccessTools.Field(typeof(LocTable), "_translations").GetValue(cardsTable) as Dictionary<string, string>
            ?? throw new InvalidOperationException("Failed to access card localization table.");

        translations[EggDescriptionLocKey] = EggDescription();
        translations[SwoopDescriptionLocKey] = swoop == null
            ? (IsChinese()
                ? "造成14点伤害[orange]1[/orange]次。"
                : "Deal 14 damage [orange]1[/orange] times.")
            : SwoopDescription(swoop);
    }

    internal static void EnsureCustomRelicLocalization()
    {
        if (LocManager.Instance == null)
        {
            return;
        }

        LocTable relicsTable = LocManager.Instance.GetTable("relics");
        Dictionary<string, string> translations =
            AccessTools.Field(typeof(LocTable), "_translations").GetValue(relicsTable) as Dictionary<string, string>
            ?? throw new InvalidOperationException("Failed to access relic localization table.");

        translations[ByrdpipDescriptionLocKey] = ByrdpipDescription();
    }

    internal static async Task UpgradeSwoopForCurrentHatch(Player owner)
    {
        List<SearingSwoopCard> swoops = GetSwoopCards(owner).ToList();
        if (swoops.Count == 0)
        {
            MainFile.Logger.Info($"No Byrd Swoop cards found for player {owner.NetId}; skipping upgrade.");
            return;
        }

        int targetUpgradeLevel = Math.Max(0, GetHatchCount(owner) - 1);
        int currentUpgradeLevel = swoops.Min(card => card.CurrentUpgradeLevel);
        int upgradesNeeded = Math.Max(0, targetUpgradeLevel - currentUpgradeLevel);
        MainFile.Logger.Info(
            $"Preparing Searing Swoop upgrade for player {owner.NetId}. Current level: {currentUpgradeLevel}, target level: {targetUpgradeLevel}, cards found: {swoops.Count}, upgrades needed: {upgradesNeeded}.");
        MainFile.Logger.Info($"Upgrade pre-state snapshot: {DescribeBirdChainState(owner)}.");

        if (upgradesNeeded == 0)
        {
            MainFile.Logger.Info($"Upgrade skipped because no additional smith steps are needed for player {owner.NetId}.");
            return;
        }

        for (int i = 0; i < upgradesNeeded; i++)
        {
            try
            {
                MainFile.Logger.Info($"Attempting smith VFX for player {owner.NetId}, upgrade step {i + 1}/{upgradesNeeded}.");
                NCardSmithVfx? smithVfx = NCardSmithVfx.Create(swoops, playSfx: true);
                if (smithVfx == null)
                {
                    MainFile.Logger.Warn($"Smith VFX creation returned null for player {owner.NetId} on upgrade step {i + 1}/{upgradesNeeded}.");
                    continue;
                }
                Task? smithTaskMaybe = Traverse.Create(smithVfx).Method("PlayAnimation", swoops).GetValue<Task>();
                Task smithTask = smithTaskMaybe ?? Task.CompletedTask;
                Task finishedTask = await Task.WhenAny(smithTask, Task.Delay(350));
                if (finishedTask != smithTask)
                {
                    MainFile.Logger.Warn($"Smith VFX soft-timeout hit for player {owner.NetId} on upgrade step {i + 1}/{upgradesNeeded}; continuing without waiting for full animation completion.");
                }
                else
                {
                    await smithTask;
                    MainFile.Logger.Info($"Smith VFX completed for player {owner.NetId}, upgrade step {i + 1}/{upgradesNeeded}.");
                }
            }
            catch (Exception exception)
            {
                MainFile.Logger.Warn($"Failed to play smith VFX for Byrd Swoop upgrade: {exception}");
            }

            using IDisposable _ = AllowInternalSwoopUpgrade();
            CardCmd.Upgrade(swoops, CardPreviewStyle.EventLayout);
            foreach (SearingSwoopCard swoop in swoops)
            {
                swoop.DynamicVars.Damage.BaseValue = GetSwoopDamageForUpgradeLevel(swoop.CurrentUpgradeLevel);
            }
            string newLevels = string.Join(", ", swoops.Select((card, index) => $"#{index + 1}:+{card.CurrentUpgradeLevel}"));
            MainFile.Logger.Info($"Upgraded Byrd Swoop for player {owner.NetId}. New target level: {targetUpgradeLevel}, step {i + 1}/{upgradesNeeded}, levels after upgrade: [{newLevels}].");
        }

        MainFile.Logger.Info($"Upgrade post-state snapshot: {DescribeBirdChainState(owner)}.");
    }

    internal static void AddStartingEggIfMissing(Player player)
    {
        if (player.RunState is not MegaCrit.Sts2.Core.Runs.RunState runState)
        {
            MainFile.Logger.Info($"Skipping starting egg injection for player {player.NetId} because RunState is not ready yet ({player.RunState?.GetType().Name ?? "null"}).");
            return;
        }

        if (HasEggCard(player))
        {
            MainFile.Logger.Info($"Player {player.NetId} already has a Byrdonis Egg in the starting deck setup; skipping extra copy.");
            return;
        }

        CardModel egg = runState.CreateCard<SearingEggCard>(player);
        player.Deck.AddInternal(egg, 0, silent: true);
        MainFile.Logger.Info($"Added Searing starting egg to player {player.NetId} at run start.");
    }
}

[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Runs.RunState), nameof(MegaCrit.Sts2.Core.Runs.RunState.CreateForNewRun))]
internal static class RunStateCreateForNewRunPatch
{
    private static void Postfix(MegaCrit.Sts2.Core.Runs.RunState __result)
    {
        if (!MainFile.IsModContentEnabled)
        {
            MainFile.Logger.Info("Skipped starting egg injection because Searing Swoop content is disabled in mod config.");
            return;
        }

        foreach (Player player in __result.Players)
        {
            SearingSwoopState.AddStartingEggIfMissing(player);
        }
    }
}

[HarmonyPatch(typeof(MegaCrit.Sts2.Core.Models.Events.ByrdonisNest), nameof(MegaCrit.Sts2.Core.Models.Events.ByrdonisNest.IsAllowed))]
internal static class ByrdonisNestIsAllowedPatch
{
    private static bool Prefix(MegaCrit.Sts2.Core.Runs.RunState runState, ref bool __result)
    {
        if (!MainFile.IsModContentEnabled)
        {
            MainFile.Logger.Info("Searing Swoop content disabled in mod config; Byrdonis Nest is left unmodified.");
            return true;
        }

        if (!SearingSwoopState.RunAlreadyHasStartingEgg(runState))
        {
            return true;
        }

        MainFile.Logger.Info("Blocked Byrdonis Nest event because the run already started with a Byrdonis Egg.");
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(HatchRestSiteOption), nameof(HatchRestSiteOption.OnSelect))]
internal static class HatchRestSiteOptionOnSelectPatch
{
    private static bool Prefix(HatchRestSiteOption __instance, ref Task<bool> __result)
    {
        __result = HandleSelect(__instance);
        return false;
    }

    private static async Task<bool> HandleSelect(HatchRestSiteOption option)
    {
        Player owner = Traverse.Create(option).Property<Player>("Owner").Value
            ?? throw new InvalidOperationException("Could not read Hatch option owner.");

        MainFile.Logger.Info($"Hatch selected for player {owner.NetId}. Existing hatch count: {GetCountForLog(owner)}.");
        MainFile.Logger.Info($"Hatch select pre-state snapshot: {SearingSwoopState.DescribeBirdChainState(owner)}.");

        RelicByrdpip? relic = owner.GetRelic<RelicByrdpip>();
        if (relic == null)
        {
            MainFile.Logger.Info("Player does not have Byrdpip yet. Obtaining relic first.");
            relic = await RelicCmd.Obtain<RelicByrdpip>(owner);
            MainFile.Logger.Info($"Byrdpip obtain completed during hatch for player {owner.NetId}. State snapshot: {SearingSwoopState.DescribeBirdChainState(owner)}.");
        }

        SearingSwoopState.IncrementHatchCount(relic);
        SearingSwoopState.RefreshRelicCounter(relic);
        relic.Flash();
        MainFile.Logger.Info($"After hatch, player {owner.NetId} relic count is {SearingSwoopState.HatchCount[relic]}.");
        MainFile.Logger.Info($"State after incrementing hatch count: {SearingSwoopState.DescribeBirdChainState(owner)}.");

        if (!SearingSwoopState.HasSwoopCard(owner))
        {
            CardModel swoop = owner.RunState.CreateCard<SearingSwoopCard>(owner);
            if (swoop is SearingSwoopCard searingSwoop)
            {
                SearingSwoopState.SyncSwoopDamage(searingSwoop);
            }
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(swoop, PileType.Deck), 2f);
            MainFile.Logger.Info("Player had no Byrd Swoop, so one was added to the deck.");
        }
        else
        {
            MainFile.Logger.Info("Player already has Byrd Swoop; no extra copy was added.");
        }

        MainFile.Logger.Info($"State before attempting Searing Swoop upgrade: {SearingSwoopState.DescribeBirdChainState(owner)}.");

        await SearingSwoopState.UpgradeSwoopForCurrentHatch(owner);
        MainFile.Logger.Info($"Hatch select post-upgrade snapshot: {SearingSwoopState.DescribeBirdChainState(owner)}.");
        MainFile.Logger.Info($"Hatch select completed for player {owner.NetId}; returning control to rest site flow.");

        return true;
    }

    private static int GetCountForLog(Player owner)
    {
        RelicByrdpip? relic = owner.GetRelic<RelicByrdpip>();
        return relic == null ? 0 : SearingSwoopState.HatchCount[relic];
    }
}

[HarmonyPatch(typeof(RelicByrdpip), nameof(RelicByrdpip.AfterObtained))]
internal static class ByrdpipAfterObtainedPatch
{
    private static bool Prefix(RelicByrdpip __instance, ref Task __result)
    {
        __result = HandleAfterObtained(__instance);
        return false;
    }

    private static async Task HandleAfterObtained(RelicByrdpip relic)
    {
        relic.Skin = new Rng((uint)(relic.Owner.NetId + relic.Owner.RunState.Rng.Seed)).NextItem(RelicByrdpip.SkinOptions)
            ?? RelicByrdpip.SkinOptions[0];
        MainFile.Logger.Info($"Byrdpip obtained for player {relic.Owner.NetId}. Current hatch count is {SearingSwoopState.HatchCount[relic]}.");

        if (CombatManager.Instance.IsInProgress)
        {
            MainFile.Logger.Info("Combat is already in progress while obtaining Byrdpip. Summoning birds immediately.");
            await SummonBirds(relic);
        }
    }

    internal static async Task SummonBirds(RelicByrdpip relic)
    {
        int count = Math.Max(1, SearingSwoopState.HatchCount[relic]);
        IReadOnlyList<string> plannedSkins = SearingSwoopState.PreparePendingBirdSkins(relic, count);
        MainFile.Logger.Info($"Summoning Byrdpips for player {relic.Owner.NetId}. Requested count: {count}. Existing pet count before summon: {relic.Owner.PlayerCombatState?.Pets.Count ?? 0}.");
        for (int i = 0; i < count; i++)
        {
            Creature pet = await PlayerCmd.AddPet<MonsterByrdpip>(relic.Owner);
            SearingSwoopState.RememberBirdSkinIfNeeded(pet, plannedSkins[Math.Min(i, plannedSkins.Count - 1)]);
        }
        MainFile.Logger.Info($"Finished summoning Byrdpips for player {relic.Owner.NetId}. Pet count after summon: {relic.Owner.PlayerCombatState?.Pets.Count ?? 0}.");
    }
}

[HarmonyPatch(typeof(RelicByrdpip), nameof(RelicByrdpip.BeforeCombatStart))]
internal static class ByrdpipBeforeCombatStartPatch
{
    private static bool Prefix(RelicByrdpip __instance, ref Task __result)
    {
        MainFile.Logger.Info($"BeforeCombatStart for Byrdpip on player {__instance.Owner.NetId}. Hatch count: {SearingSwoopState.HatchCount[__instance]}.");
        __result = ByrdpipAfterObtainedPatch.SummonBirds(__instance);
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.ShowCounter), MethodType.Getter)]
internal static class ByrdpipShowCounterPatch
{
    private static bool Prefix(RelicModel __instance, ref bool __result)
    {
        if (__instance is not RelicByrdpip byrdpip)
        {
            return true;
        }

        __result = SearingSwoopState.HatchCount[byrdpip] > 0;
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.DisplayAmount), MethodType.Getter)]
internal static class ByrdpipDisplayAmountPatch
{
    private static bool Prefix(RelicModel __instance, ref int __result)
    {
        if (__instance is not RelicByrdpip byrdpip)
        {
            return true;
        }

        __result = SearingSwoopState.HatchCount[byrdpip];
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), nameof(RelicModel.Title), MethodType.Getter)]
internal static class ByrdpipTitlePatch
{
    private static bool Prefix(RelicModel __instance, ref LocString __result)
    {
        if (__instance is not RelicByrdpip byrdpip || SearingSwoopState.HatchCount[byrdpip] <= 1)
        {
            return true;
        }

        __result = SearingSwoopState.ByrdpipPluralRelicTitle();
        return false;
    }
}

[HarmonyPatch(typeof(RelicModel), "Description", MethodType.Getter)]
internal static class ByrdpipDescriptionPatch
{
    private static bool Prefix(RelicModel __instance, ref LocString __result)
    {
        if (__instance is not RelicByrdpip)
        {
            return true;
        }

        __result = SearingSwoopState.ByrdpipDescriptionLocString();
        return false;
    }
}

[HarmonyPatch(typeof(MonsterByrdpip), nameof(MonsterByrdpip.SetupSkins))]
internal static class ByrdpipSetupSkinsPatch
{
    private static void Prefix(MonsterByrdpip __instance, ref string? __state)
    {
        Creature? creature = __instance.Creature;
        Player? owner = creature?.PetOwner;
        RelicByrdpip? relic = owner?.GetRelic<RelicByrdpip>();
        if (creature == null || relic == null)
        {
            return;
        }

        __state = relic.Skin;
        relic.Skin = SearingSwoopState.GetOrAssignBirdSkin(creature, relic);
        MainFile.Logger.Info($"Using Byrdpip skin '{relic.Skin}' while setting up visuals for creature {creature.CombatId?.ToString() ?? "no-combat-id"}.");
    }

    private static void Postfix(MonsterByrdpip __instance, string? __state)
    {
        Creature? creature = __instance.Creature;
        Player? owner = creature?.PetOwner;
        RelicByrdpip? relic = owner?.GetRelic<RelicByrdpip>();
        if (relic == null || string.IsNullOrWhiteSpace(__state))
        {
            return;
        }

        relic.Skin = __state;
    }
}

[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.PositionPlayersAndPets))]
internal static class ByrdpipPositionSpreadPatch
{
    // Keep vanilla layout as baseline, then add a small right-side expansion.
    private const float RightEdgeExpansionPixels = 36f;

    private static void Postfix(List<NCreature> creatureNodes)
    {
        if (creatureNodes == null || creatureNodes.Count == 0)
        {
            return;
        }

        List<NCreature> byrdNodes = creatureNodes
            .Where(IsByrdPetNode)
            .OrderBy(node => node.Position.X)
            .ToList();

        if (byrdNodes.Count <= 1)
        {
            return;
        }

        float minBefore = byrdNodes.Min(node => node.Position.X);
        float maxBefore = byrdNodes.Max(node => node.Position.X);
        float widthBefore = maxBefore - minBefore;
        if (widthBefore <= 0.01f)
        {
            return;
        }

        float scale = (widthBefore + RightEdgeExpansionPixels) / widthBefore;
        foreach (NCreature node in byrdNodes)
        {
            float relativeX = node.Position.X - minBefore;
            float expandedX = minBefore + (relativeX * scale);
            node.Position = node.Position with { X = expandedX };
        }

        float minAfter = byrdNodes.Min(node => node.Position.X);
        float maxAfter = byrdNodes.Max(node => node.Position.X);
        MainFile.Logger.Info(
            $"Byrdpip position range adjusted (count={byrdNodes.Count}): X [{minBefore:0.##}, {maxBefore:0.##}] -> [{minAfter:0.##}, {maxAfter:0.##}], right expansion={RightEdgeExpansionPixels:0.##}.");
    }

    private static bool IsByrdPetNode(NCreature node)
    {
        Creature? entity = node.Entity;
        return entity?.IsPet == true
            && entity.PetOwner != null
            && entity.Monster is MonsterByrdpip;
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.Title), MethodType.Getter)]
internal static class SearingCardTitlePatch
{
    private static bool Prefix(CardModel __instance, ref string __result)
    {
        switch (__instance)
        {
            case SearingEggCard:
                __result = SearingSwoopState.EggTitle();
                return false;
            case SearingSwoopCard swoop:
                __result = SearingSwoopState.SwoopTitle(swoop);
                return false;
            default:
                return true;
        }
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.Description), MethodType.Getter)]
internal static class SearingCardDescriptionLocPatch
{
    private static bool Prefix(CardModel __instance, ref LocString __result)
    {
        switch (__instance)
        {
            case SearingEggCard:
                __result = SearingSwoopState.EggDescriptionLocString();
                return false;
            case SearingSwoopCard swoop:
                __result = SearingSwoopState.SwoopDescriptionLocString(swoop);
                return false;
            default:
                return true;
        }
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForPile), [typeof(PileType), typeof(Creature)])]
internal static class SearingCardDescriptionPatch
{
    private static bool Prefix(CardModel __instance, PileType pileType, Creature? target, ref string __result)
    {
        switch (__instance)
        {
            case SearingEggCard:
                __result = SearingSwoopState.EggDescription();
                return false;
            case SearingSwoopCard swoop:
                __result = SearingSwoopState.SwoopDescription(swoop);
                return false;
            default:
                return true;
        }
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.GetDescriptionForUpgradePreview))]
internal static class SearingCardUpgradeDescriptionPatch
{
    private static bool Prefix(CardModel __instance, ref string __result)
    {
        switch (__instance)
        {
            case SearingEggCard:
                __result = SearingSwoopState.EggDescription();
                return false;
            case SearingSwoopCard swoop:
                __result = SearingSwoopState.SwoopDescription(swoop, previewNextUpgrade: true);
                return false;
            default:
                return true;
        }
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.MaxUpgradeLevel), MethodType.Getter)]
internal static class SearingCardMaxUpgradePatch
{
    private static bool Prefix(CardModel __instance, ref int __result)
    {
        switch (__instance)
        {
            case SearingEggCard egg:
                __result = egg.CurrentUpgradeLevel;
                return false;
            case SearingSwoopCard swoop:
                __result = SearingSwoopState.CanBypassSwoopUpgradeLock()
                    ? int.MaxValue / 4
                    : swoop.CurrentUpgradeLevel;
                return false;
            default:
                return true;
        }
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.CurrentUpgradeLevel), MethodType.Setter)]
internal static class SearingSwoopUpgradeLevelLockPatch
{
    private static void Prefix(CardModel __instance, ref int value)
    {
        if (__instance is not SearingSwoopCard swoop)
        {
            return;
        }

        if (SearingSwoopState.CanBypassSwoopUpgradeLock())
        {
            return;
        }

        int lockedLevel = SearingSwoopState.GetLockedSwoopUpgradeLevel(swoop);
        if (value == lockedLevel)
        {
            return;
        }

        MainFile.Logger.Info(
            $"Blocked external Searing Swoop level change: requested={value}, locked={lockedLevel}, owner={swoop.Owner?.NetId.ToString() ?? "null"}.");
        value = lockedLevel;
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.FromSerializable))]
internal static class SearingSwoopDeserializeScopePatch
{
    private static void Prefix()
    {
        SearingSwoopState.EnterSwoopDeserializeUpgradeScope();
    }

    private static void Postfix()
    {
        SearingSwoopState.ExitSwoopDeserializeUpgradeScope();
    }
}
