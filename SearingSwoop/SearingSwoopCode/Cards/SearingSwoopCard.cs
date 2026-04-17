using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace SearingSwoop.SearingSwoopCode.Cards;

public class SearingSwoopCard() : SearingEventCard(0, CardType.Attack, CardRarity.Event, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(14, ValueProp.Move),
        new RepeatVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        int totalHits = Math.Max(1, SearingSwoopState.GetBirdPets(Owner).Count);
        IReadOnlyList<Creature> birds = SearingSwoopState.GetBirdPets(Owner);
        int damage = SearingSwoopState.GetSwoopDamageForUpgradeLevel(CurrentUpgradeLevel);
        float attackAnimDelay = Owner?.Character?.AttackAnimDelay ?? 0f;

        DynamicVars.Damage.BaseValue = damage;
        DynamicVars.Repeat.BaseValue = totalHits;
        MainFile.Logger.Info($"Playing Searing Swoop for player {Owner?.NetId}. Upgrade level: {CurrentUpgradeLevel}. Damage per hit: {damage}. Total hits: {totalHits}. Bird pets found: {birds.Count}.");

        for (int i = 0; i < totalHits; i++)
        {
            Creature? attacker = birds.Count == 0 ? null : birds[i % birds.Count];
            MainFile.Logger.Info($"Executing Searing Swoop hit {i + 1}/{totalHits}. Attacker pet present: {attacker != null}.");

            await DamageCmd.Attack(damage)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithAttackerAnim("Attack", attackAnimDelay, attacker)
                .WithHitFx("vfx/vfx_attack_slash", ByrdSwoop.attackSfx)
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.BaseValue = SearingSwoopState.GetSwoopDamageForUpgradeLevel(CurrentUpgradeLevel + 1);
        MainFile.Logger.Info($"Searing Swoop OnUpgrade invoked. New preview damage: {DynamicVars.Damage.BaseValue}, resulting level: {CurrentUpgradeLevel + 1}.");
    }
}
