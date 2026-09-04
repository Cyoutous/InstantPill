using System;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;

namespace InstantPill.InstantPillCode.Gameplay.Effects;

/// <summary>
/// Per-player, run-persistent counter for Puberty uses. It deliberately belongs to the player,
/// not a card instance, because Puberty cards purge after play and may be obtained repeatedly.
/// </summary>
public static class PubertyPlayCounter
{
    public static SavedSpireField<Player, int> Count { get; } =
        new(_ => 0, "instant_pill_puberty_play_count");

    public static int Increment(Player player)
    {
        int previousCount = Count.Get(player);
        if (previousCount < 0 || previousCount == int.MaxValue)
        {
            throw new InvalidOperationException("InstantPill Puberty play count is invalid.");
        }

        int currentCount = previousCount + 1;
        Count.Set(player, currentCount);
        return currentCount;
    }
}
