using System;
using BaseLib.Utils;
using InstantPill.InstantPillCode.Configuration;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>
/// Owns the host-authoritative configuration snapshot used by a multiplayer run. The pending
/// value is only a short-lived lobby handoff; all in-run code reads the saved session state.
/// </summary>
public static class PillMultiplayerRulesService
{
    private static PillMultiplayerRulesSnapshot? _pendingLobbySnapshot;

    public static SavedSpireField<IRunState, PillMultiplayerRulesSessionState> State { get; } =
        new(_ => null, "instant_pill_multiplayer_rules");

    public static void PrepareHostLobbySnapshot(INetGameService netService)
    {
        if (netService.Type != NetGameType.Host)
        {
            return;
        }

        bool synchronizeParameters = PillRewardSettings.GetValue(
            PillRewardSettings.SyncMultiplayerParametersWithHostKey) is true;
        bool enableSharedPool = synchronizeParameters &&
            PillRewardSettings.GetValue(PillRewardSettings.EnableSharedMultiplayerPillPoolKey) is true;
        PillRewardRulesSnapshot rules = PillRewardSettings.CreateRulesForNewRun(PillPoolRules.PoolSize);

        _pendingLobbySnapshot = new PillMultiplayerRulesSnapshot
        {
            HostSynchronizesParameters = synchronizeParameters,
            HostEnablesSharedPillPool = enableSharedPool,
            RewardRules = rules
        };
        MainFile.Logger.Info(
            $"Prepared InstantPill multiplayer host snapshot: synchronize={synchronizeParameters}, sharedPool={enableSharedPool}.",
            1);
    }

    public static void WriteLobbySnapshot(MegaCrit.Sts2.Core.Multiplayer.Serialization.PacketWriter writer)
    {
        bool hasSnapshot = _pendingLobbySnapshot != null;
        writer.WriteBool(hasSnapshot);
        if (hasSnapshot)
        {
            _pendingLobbySnapshot!.Serialize(writer);
        }
    }

    public static void ReadLobbySnapshot(MegaCrit.Sts2.Core.Multiplayer.Serialization.PacketReader reader)
    {
        if (!reader.ReadBool())
        {
            _pendingLobbySnapshot = null;
            return;
        }

        _pendingLobbySnapshot = PillMultiplayerRulesSnapshot.Deserialize(reader);
    }

    /// <summary>
    /// Must execute before new-run deck initialization. The lobby begin message has already been
    /// decoded on clients by this point, so no client-local setting can influence the session.
    /// </summary>
    public static void InitializeNewMultiplayerRun(IRunState runState, INetGameService netService)
    {
        if (State.Get(runState) != null)
        {
            return;
        }

        PillMultiplayerRulesSnapshot snapshot = _pendingLobbySnapshot
            ?? CreateSafeFallbackSnapshot(netService);
        _pendingLobbySnapshot = null;
        State.Set(runState, new PillMultiplayerRulesSessionState
        {
            Snapshot = snapshot
        });

        MainFile.Logger.Info(
            $"Initialized InstantPill multiplayer settings: synchronize={snapshot.HostSynchronizesParameters}, sharedPool={snapshot.HostEnablesSharedPillPool}.",
            1);
    }

    public static PillRewardRulesSnapshot CreateRulesForNewPlayer(Player player)
    {
        return CreateRulesForNewRun(player.RunState);
    }

    /// <summary>
    /// Returns the rules that govern this newly-created run before player-owned reward state has
    /// been initialized. This is also used while vanilla constructs relic grab bags.
    /// </summary>
    public static PillRewardRulesSnapshot CreateRulesForNewRun(IRunState runState)
    {
        PillMultiplayerRulesSessionState? state = State.Get(runState);
        if (state?.Snapshot.HostSynchronizesParameters == true)
        {
            return state.Snapshot.RewardRules.Clone();
        }

        return PillRewardSettings.CreateRulesForNewRun(PillPoolRules.PoolSize);
    }

    public static bool IsSharedPillPoolEnabled(IRunState runState)
    {
        PillMultiplayerRulesSessionState? state = State.Get(runState);
        return state?.Snapshot is
        {
            HostSynchronizesParameters: true,
            HostEnablesSharedPillPool: true
        };
    }

    private static PillMultiplayerRulesSnapshot CreateSafeFallbackSnapshot(INetGameService netService)
    {
        if (netService.Type == NetGameType.Host)
        {
            // This is only a safeguard for an unexpected lobby-message ordering issue. The host
            // still uses its own setting; a client never substitutes its local preferences here.
            PrepareHostLobbySnapshot(netService);
            return _pendingLobbySnapshot
                ?? throw new InvalidOperationException("InstantPill could not create its multiplayer host snapshot.");
        }

        MainFile.Logger.Warn(
            "InstantPill did not receive a multiplayer host settings snapshot. Falling back to independent local settings.",
            1);
        return new PillMultiplayerRulesSnapshot();
    }
}
