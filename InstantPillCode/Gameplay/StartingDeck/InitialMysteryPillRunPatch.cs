using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using InstantPill.InstantPillCode.Gameplay.Pools;
using InstantPill.InstantPillCode.Gameplay.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.StartingDeck;

/// <summary>
/// Runs only for the engine's new-run setup methods. Saved runs use SetUpSaved* and therefore
/// cannot receive the configured deck additions a second time.
/// </summary>
[HarmonyPatch]
internal static class InitialMysteryPillRunPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // The public-beta assembly spells "Singleplayer"/"Multiplayer" as one word. Resolve
        // by its binary name so this source keeps compiling even though that API is non-public
        // in the reference assembly used by the mod SDK.
        yield return RequireMethod("SetUpNewSingleplayer");
        yield return RequireMethod("SetUpNewMultiplayer");
        yield return RequireMethod("SetUpTest");
    }

    [HarmonyPostfix]
    private static void GrantStartingMysteryPills(RunState __0)
    {
        foreach (var player in __0.Players)
        {
            InitialMysteryPillService.GrantForNewRun(player);
        }
    }

    // Take the run-level mode snapshot before this setup method creates starting decks. The
    // postfix below subsequently calls into the pool service to roll each player's configured
    // starting mystery pills, so waiting until RunStarted would be too late.
    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    private static void SnapshotPoolModeForNewRun(RunState __0) =>
        PillPoolService.InitializeRunState(__0);

    private static MethodBase RequireMethod(string methodName) =>
        AccessTools.Method(typeof(RunManager), methodName)
        ?? throw new MissingMethodException(typeof(RunManager).FullName, methodName);
}
