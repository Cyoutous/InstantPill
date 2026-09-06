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
/// Defers one doubling per stack to the victory reward phase. Luck Up and Luck Down stacks are
/// combined there, before either multiplier is applied to an integer Gold reward.
/// </summary>
[CustomID("INSTANTPILL-LUCK_UP")]
public sealed class LuckUpPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Temporary presentation until InstantPill receives a dedicated power icon.
    public override string? CustomPackedIconPath => ModelDb.Power<DoubleDamagePower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        Player? player = Owner.Player;
        if (player != null && Amount > 0m)
        {
            LuckDownRewardState.QueueDoubles(player, decimal.ToInt32(Amount));
        }

        return Task.CompletedTask;
    }
}
