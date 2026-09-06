using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Gameplay.Rewards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace InstantPill.InstantPillCode.Powers;

/// <summary>
/// Defers one halve per stack to the reward phase of this combat's victory. Powers are cleared
/// before rewards exist, so AfterCombatEnd records the stack count in LuckDownRewardState.
/// </summary>
[CustomID("INSTANTPILL-LUCK_DOWN")]
public sealed class LuckDownPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Temporary presentation until InstantPill receives a dedicated power icon.
    public override string? CustomPackedIconPath => ModelDb.Power<KnockdownPower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<KnockdownPower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<KnockdownPower>().ResolvedBigIconPath;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        Player? player = Owner.Player;
        if (player != null && Amount > 0m)
        {
            LuckDownRewardState.QueueHalves(player, decimal.ToInt32(Amount));
        }

        return Task.CompletedTask;
    }
}
