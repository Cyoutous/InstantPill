using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using InstantPill.InstantPillCode.Gameplay.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.PHD;

/// <summary>Scopes PHD's configured rarity across vanilla new-run relic-grab-bag construction.</summary>
[HarmonyPatch]
internal static class PhdRelicRarityRunSetupPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return RequireMethod("SetUpNewSingleplayer");
        yield return RequireMethod("SetUpNewMultiplayer");
        yield return RequireMethod("SetUpTest");
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    private static void ApplyConfiguredRarity(RunState __0, ref IDisposable? __state)
    {
        PillRewardRulesSnapshot rules = PillMultiplayerRulesService.CreateRulesForNewRun(__0);
        __state = new RarityScope(
            PhdRelicRarityService.PushNewRunRules(rules),
            FalsePhdRelicRarityService.PushNewRunRules(rules));
    }

    [HarmonyPostfix]
    private static void ClearConfiguredRarity(IDisposable? __state) => __state?.Dispose();

    [HarmonyFinalizer]
    private static Exception? ClearConfiguredRarityOnFailure(Exception? __exception, IDisposable? __state)
    {
        __state?.Dispose();
        return __exception;
    }

    private static MethodBase RequireMethod(string methodName) =>
        AccessTools.Method(typeof(RunManager), methodName)
        ?? throw new MissingMethodException(typeof(RunManager).FullName, methodName);

    private sealed class RarityScope(params IDisposable[] scopes) : IDisposable
    {
        private IDisposable[]? _scopes = scopes;

        public void Dispose()
        {
            if (_scopes is not { } activeScopes)
            {
                return;
            }

            _scopes = null;

            for (int index = activeScopes.Length - 1; index >= 0; index--)
            {
                activeScopes[index].Dispose();
            }
        }
    }
}
