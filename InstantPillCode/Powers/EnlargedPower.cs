using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Powers;

/// <summary>
/// A one-turn-per-stack offensive size increase. This deliberately mirrors ShrinkPower's damage
/// filter while applying the inverse multiplier to powered attacks made by its owner.
/// </summary>
[CustomID("INSTANTPILL-ENLARGED")]
public sealed class EnlargedPower : CustomPowerModel
{
    private const float EnlargedScale = 1.5f;
    private const float ScaleAnimationDuration = 0.75f;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Temporary presentation until InstantPill receives dedicated power icons.
    public override string? CustomPackedIconPath => ModelDb.Power<DoubleDamagePower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        NCombatRoom.Instance?.GetCreatureNode(Owner)?.ScaleTo(EnlargedScale, ScaleAnimationDuration);
        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        NCombatRoom.Instance?.GetCreatureNode(oldOwner)?.ScaleTo(1f, ScaleAnimationDuration);
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Decrement(this);
        }
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner || !props.IsPoweredAttack())
        {
            return 1m;
        }

        return 1.5m;
    }
}
