using System;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>
/// Carries Luck Down and Luck Up stacks from the final live-combat hook to the later
/// reward-generation phase, after combat powers have been cleared. The values are consumed by
/// that one room's reward generation and are persisted in case the run is saved in between phases.
/// </summary>
public static class LuckDownRewardState
{
    public static SavedSpireField<Player, int> PendingHalves { get; } =
        new(_ => 0, "instant_pill_luck_down_pending_halves");

    public static SavedSpireField<Player, int> PendingDoubles { get; } =
        new(_ => 0, "instant_pill_luck_up_pending_doubles");

    public static void QueueHalves(Player player, int stackCount) =>
        Queue(player, PendingHalves, stackCount, "Luck Down");

    public static void QueueDoubles(Player player, int stackCount) =>
        Queue(player, PendingDoubles, stackCount, "Luck Up");

    public static int ConsumeHalves(Player player) => Consume(player, PendingHalves);

    public static int ConsumeDoubles(Player player) => Consume(player, PendingDoubles);

    private static void Queue(
        Player player,
        SavedSpireField<Player, int> field,
        int stackCount,
        string effectName)
    {
        if (stackCount <= 0)
        {
            return;
        }

        int existing = field.Get(player);
        if (existing < 0 || existing > int.MaxValue - stackCount)
        {
            throw new InvalidOperationException($"InstantPill {effectName} reward state is invalid.");
        }

        field.Set(player, existing + stackCount);
    }

    private static int Consume(Player player, SavedSpireField<Player, int> field)
    {
        int pending = field.Get(player);
        field.Set(player, 0);
        return Math.Max(0, pending);
    }
}

/// <summary>Registers Luck Down's primitive per-player deferred reward state with BaseLib saves.</summary>
public static class LuckDownRewardSaveRegistration
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        if (!LuckDownRewardState.PendingHalves.RegisterCustomSave())
        {
            throw new InvalidOperationException("InstantPill could not register the Luck Down reward save field.");
        }

        if (!LuckDownRewardState.PendingDoubles.RegisterCustomSave())
        {
            throw new InvalidOperationException("InstantPill could not register the Luck Up reward save field.");
        }

        _registered = true;
    }
}
