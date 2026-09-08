using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Messages.Lobby;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>
/// Extends the one authoritative lobby-begin packet with InstantPill's host settings snapshot.
/// This packet is sent before each client constructs its new run, making it safe to synchronize
/// starting-deck parameters as well as combat-reward parameters.
/// </summary>
internal static class PillMultiplayerRulesLobbyPatch
{
    [HarmonyPatch(typeof(StartRunLobby), "BeginRunForAllPlayers")]
    [HarmonyPrefix]
    private static void PrepareHostSnapshot(StartRunLobby __instance) =>
        PillMultiplayerRulesService.PrepareHostLobbySnapshot(__instance.NetService);

    [HarmonyPatch(typeof(LobbyBeginRunMessage), nameof(LobbyBeginRunMessage.Serialize))]
    [HarmonyPostfix]
    private static void AppendHostSnapshot(PacketWriter writer) =>
        PillMultiplayerRulesService.WriteLobbySnapshot(writer);

    [HarmonyPatch(typeof(LobbyBeginRunMessage), nameof(LobbyBeginRunMessage.Deserialize))]
    [HarmonyPostfix]
    private static void ReadHostSnapshot(PacketReader reader) =>
        PillMultiplayerRulesService.ReadLobbySnapshot(reader);
}
