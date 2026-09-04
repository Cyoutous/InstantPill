using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>Inserts capsule rewards only after vanilla combat rewards, including potions, exist.</summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.ModifyRewards))]
internal static class PillRewardGenerationPatch
{
    [HarmonyPostfix]
    private static void AddCapsuleCombatRewards(
        IRunState runState,
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        PillRewardService.AddCombatRewards(player, rewards, room);
    }
}
