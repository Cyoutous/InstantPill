using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>Consumes the lobby snapshot before any new-run deck or reward initialization runs.</summary>
[HarmonyPatch(typeof(RunManager), "SetUpNewMultiplayer")]
internal static class PillMultiplayerRulesRunSetupPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void InitializeHostRules(RunState state, StartRunLobby lobby) =>
        PillMultiplayerRulesService.InitializeNewMultiplayerRun(state, lobby.NetService);
}
