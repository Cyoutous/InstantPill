using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Powers;

/// <summary>
/// Makes the owner's draw-pile viewer preserve its real draw sequence for this combat.
/// The single stack is deliberately immutable: reapplying it only refreshes its visual feedback.
/// </summary>
[CustomID("INSTANTPILL-I_CAN_SEE_FOREVER")]
public sealed class ICanSeeForeverPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // Temporary presentation until InstantPill receives a dedicated power icon.
    public override string? CustomPackedIconPath => ModelDb.Power<DoubleDamagePower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    /// <summary>
    /// A second application must not create a hidden amount of two or more. This hook only runs
    /// after this instance is already on its owner's power list, so it does not affect its first
    /// application.
    /// </summary>
    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        out decimal modifiedAmount)
    {
        if (canonicalPower is ICanSeeForeverPower && target == Owner)
        {
            modifiedAmount = 0m;
            return true;
        }

        modifiedAmount = amount;
        return false;
    }
}
