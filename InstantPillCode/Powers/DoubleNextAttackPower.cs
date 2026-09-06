using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Powers;

/// <summary>
/// Doubles every powered attack produced by the first eligible card play, then consumes one stack
/// only after that entire card play completes. This deliberately preserves the multiplier across
/// a card's multi-hit attack command(s).
/// </summary>
[CustomID("INSTANTPILL-DOUBLE_NEXT_ATTACK")]
public sealed class DoubleNextAttackPower : CustomPowerModel
{
    private CardPlay? boostedCardPlay;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Temporary presentation until InstantPill receives a dedicated power icon.
    public override string? CustomPackedIconPath => ModelDb.Power<DoubleDamagePower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    public override Task BeforeAttack(AttackCommand command)
    {
        if (boostedCardPlay is not null
            || command.CardPlay is null
            || command.ModelSource is not CardModel sourceCard
            || sourceCard.Owner.Creature != Owner
            || !command.DamageProps.IsPoweredAttack())
        {
            return Task.CompletedTask;
        }

        boostedCardPlay = command.CardPlay;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return cardPlay == boostedCardPlay && props.IsPoweredAttack() ? 2m : 1m;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay != boostedCardPlay)
        {
            return;
        }

        // Clear first so no later hook during removal can consume a second stack.
        boostedCardPlay = null;
        await PowerCmd.Decrement(this);
    }
}
