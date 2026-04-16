using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;
using SearingSwoop.SearingSwoopCode.Extensions;
using SearingSwoop.SearingSwoopCode.Utils;

namespace SearingSwoop.SearingSwoopCode.Cards;

[Pool(typeof(EventCardPool))]
public abstract class SearingEventCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    private string PortraitFileName => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    // Use runtime-loaded texture to avoid exported runtime "no loader for png" issues.
    public override string? CustomPortraitPath => null;
    public override Texture2D? CustomPortrait => CardPortraitLoader.LoadPortrait(PortraitFileName);
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => PortraitPath;
}
