using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;

namespace SearingSwoop.SearingSwoopCode.Cards;

public class SearingEggCard() : SearingEventCard(-1, CardType.Quest, CardRarity.Quest, TargetType.None)
{
    public override int MaxUpgradeLevel => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        if (player != Owner)
        {
            return false;
        }

        if (!options.Any(option => option is HatchRestSiteOption))
        {
            options.Add(new HatchRestSiteOption(player));
            MainFile.Logger.Info($"Added Hatch rest site option from Searing Egg for player {player.NetId}. State snapshot: {SearingSwoopState.DescribeBirdChainState(player)}.");
        }
        return true;
    }
}
