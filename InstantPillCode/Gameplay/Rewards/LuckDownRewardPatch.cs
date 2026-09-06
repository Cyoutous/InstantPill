using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>
/// Applies and consumes deferred Luck Down / Luck Up stacks once populated combat rewards exist.
/// Opposite stacks cancel before integer rounding occurs.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyRewards))]
internal static class LuckDownRewardPatch
{
    [HarmonyPostfix]
    private static void ModifyCombatGoldRewards(
        IRunState runState,
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        if (room is not CombatRoom)
        {
            return;
        }

        int halves = LuckDownRewardState.ConsumeHalves(player);
        int doubles = LuckDownRewardState.ConsumeDoubles(player);
        int netDoublings = doubles - halves;
        if (netDoublings == 0)
        {
            return;
        }

        for (int rewardIndex = 0; rewardIndex < rewards.Count; rewardIndex++)
        {
            if (rewards[rewardIndex] is not GoldReward goldReward)
            {
                continue;
            }

            int modifiedAmount = goldReward.Amount;
            if (netDoublings > 0)
            {
                for (int doubleIndex = 0; doubleIndex < netDoublings; doubleIndex++)
                {
                    modifiedAmount = (int)System.Math.Min(int.MaxValue, (long)modifiedAmount * 2L);
                }
            }
            else
            {
                for (int halveIndex = 0; halveIndex < -netDoublings; halveIndex++)
                {
                    modifiedAmount /= 2;
                }
            }

            // GoldReward.Amount is populated and intentionally read-only. Replace the populated
            // reward to keep its original reward-screen behavior while using the reduced amount.
            rewards[rewardIndex] = new GoldReward(modifiedAmount, player);
        }

        MainFile.Logger.Info(
            $"Applied Luck reward modifier: doubles={doubles}, halves={halves}, net={netDoublings} for player {player.NetId}.",
            1);
    }
}
