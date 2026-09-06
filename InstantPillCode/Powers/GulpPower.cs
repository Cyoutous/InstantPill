using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace InstantPill.InstantPillCode.Powers;

/// <summary>
/// Converts every stack into one normal relic reward once this combat is won.
/// CombatRoom owns and serializes the extra rewards, so they survive until the player sees them.
/// </summary>
[CustomID("INSTANTPILL-GULP")]
public sealed class GulpPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Temporary presentation until InstantPill receives a dedicated power icon.
    public override string? CustomPackedIconPath => ModelDb.Power<DoubleDamagePower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<DoubleDamagePower>().ResolvedBigIconPath;

    // The victory sequence removes all combat powers before AfterCombatVictory runs. AfterCombatEnd
    // is the final victory-only combat hook that still includes this power in its listener list.
    public override Task AfterCombatEnd(CombatRoom room)
    {
        Player? player = Owner.Player;
        if (player is null)
        {
            return Task.CompletedTask;
        }

        for (int rewardIndex = 0; rewardIndex < Amount; rewardIndex++)
        {
            // A normal RelicReward pulls from the player's standard relic pool when rewards populate.
            room.AddExtraReward(player, new RelicReward(player));
        }

        return Task.CompletedTask;
    }
}
