using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Gameplay.Intents;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Powers;

/// <summary>
/// A non-stacking combat debuff which removes enemy-intent presentation for its local owner.
/// </summary>
[CustomID("INSTANTPILL-AMNESIA")]
public sealed class AmnesiaPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    // Reuse the original Confused ("Confusion") power icon until bespoke art is available.
    public override string? CustomPackedIconPath => ModelDb.Power<ConfusedPower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<ConfusedPower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<ConfusedPower>().ResolvedBigIconPath;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (LocalContext.IsMe(Owner))
        {
            AmnesiaIntentVisibilityPatch.Refresh(Owner.CombatState);
        }

        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (LocalContext.IsMe(oldOwner))
        {
            AmnesiaIntentVisibilityPatch.Refresh(oldOwner.CombatState);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Reapplying Amnesia leaves the existing single stack unchanged.
    /// </summary>
    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        out decimal modifiedAmount)
    {
        if (canonicalPower is AmnesiaPower && target == Owner)
        {
            modifiedAmount = 0m;
            return true;
        }

        modifiedAmount = amount;
        return false;
    }
}
