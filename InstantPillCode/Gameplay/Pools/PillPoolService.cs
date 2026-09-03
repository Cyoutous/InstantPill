using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using InstantPill.InstantPillCode;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// Sole read/write entry point for InstantPill's two run-local pools.
/// </summary>
public static class PillPoolService
{
    private static readonly ModelId PoolRngId = new("INSTANTPILL", "PILL_POOL");

    public static SavedSpireField<Player, PillPoolState> State { get; } =
        new(_ => null, "instant_pill_pool");

    /// <summary>
    /// Creates this player's pools only when no saved state exists. A restored state is validated,
    /// never rerolled.
    /// </summary>
    public static PillPoolState EnsureInitialized(Player player)
    {
        PillPoolState? existingState = State.Get(player);
        if (existingState != null)
        {
            ValidateExistingState(existingState);
            return existingState;
        }

        PillPoolRules.ValidateCatalog();

        Rng rng = new(player, PoolRngId);
        List<string> selectedEffects = new(PillPoolCatalog.EffectPillIds);
        List<string> selectedMysteries = new(PillPoolCatalog.MysteryPillIds);
        rng.Shuffle(selectedEffects);
        rng.Shuffle(selectedMysteries);

        PillPoolState state = new()
        {
            RemainingEffectIds = selectedEffects.Take(PillPoolRules.PoolSize).ToList(),
            CapsuleSlots = selectedMysteries
                .Take(PillPoolRules.PoolSize)
                .Select(mysteryId => new PillPoolSlotState { MysteryPillId = mysteryId })
                .ToList()
        };

        State.Set(player, state);
        MainFile.Logger.Info(
            $"Initialized InstantPill pools for player {player.NetId}: {state.CapsuleSlots.Count} capsule slots and {state.RemainingEffectIds.Count} candidate effects.",
            1);
        return state;
    }

    /// <summary>
    /// Randomly selects a capsule-pool slot without removing it. The returned entry is a mystery
    /// card before that slot is revealed, or its mapped effect card afterward.
    /// </summary>
    public static string RollCapsuleCardId(Player player)
    {
        PillPoolState state = EnsureInitialized(player);
        if (state.CapsuleSlots.Count == 0)
        {
            throw new InvalidOperationException("InstantPill cannot roll a capsule from an empty capsule pool.");
        }

        Rng rng = NextPoolRng(player, state);
        PillPoolSlotState slot = state.CapsuleSlots[rng.NextInt(state.CapsuleSlots.Count)];
        State.Set(player, state);
        return slot.CurrentCardId;
    }

    /// <summary>
    /// Returns an existing mapping or reveals one remaining candidate effect for this mystery
    /// identity. A null return means the supplied card is not a member of this player's capsule pool.
    /// </summary>
    public static string? RevealMysteryPill(Player player, string mysteryPillId)
    {
        PillPoolState state = EnsureInitialized(player);
        PillPoolSlotState? slot = state.CapsuleSlots
            .FirstOrDefault(candidate => candidate.MysteryPillId == mysteryPillId);

        if (slot == null)
        {
            return null;
        }

        if (slot.RevealedEffectPillId != null)
        {
            return slot.RevealedEffectPillId;
        }

        if (state.RemainingEffectIds.Count == 0)
        {
            throw new InvalidOperationException(
                $"InstantPill has no candidate effect remaining to reveal {mysteryPillId}.");
        }

        Rng rng = NextPoolRng(player, state);
        int effectIndex = rng.NextInt(state.RemainingEffectIds.Count);
        string revealedEffectId = state.RemainingEffectIds[effectIndex];
        state.RemainingEffectIds.RemoveAt(effectIndex);
        slot.RevealedEffectPillId = revealedEffectId;
        State.Set(player, state);

        MainFile.Logger.Info($"Revealed {mysteryPillId} as {revealedEffectId} for player {player.NetId}.", 1);
        return revealedEffectId;
    }

    public static string? TryGetRevealedEffectCardId(Player player, string mysteryPillId)
    {
        PillPoolState state = EnsureInitialized(player);
        return state.CapsuleSlots
            .FirstOrDefault(slot => slot.MysteryPillId == mysteryPillId)
            ?.RevealedEffectPillId;
    }

    private static Rng NextPoolRng(Player player, PillPoolState state)
    {
        if (state.RandomRollCounter < 0)
        {
            throw new InvalidOperationException("InstantPill saved pool RNG counter is invalid.");
        }

        ulong mixin = (ulong)state.RandomRollCounter;
        state.RandomRollCounter++;
        return new Rng(player, PoolRngId, mixin);
    }

    private static void ValidateExistingState(PillPoolState state)
    {
        if (state.SchemaVersion != PillPoolRules.SchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported InstantPill pool-state schema {state.SchemaVersion}. Expected {PillPoolRules.SchemaVersion}.");
        }

        if (state.RandomRollCounter < 0)
        {
            throw new InvalidOperationException("InstantPill saved pool RNG counter is invalid.");
        }

        if (state.CapsuleSlots.Count == 0 || state.CapsuleSlots.Count > PillPoolCatalog.MysteryPillIds.Count)
        {
            throw new InvalidOperationException("InstantPill saved capsule pool has an invalid slot count.");
        }

        if (state.CapsuleSlots.Select(slot => slot.MysteryPillId).Distinct().Count() != state.CapsuleSlots.Count)
        {
            throw new InvalidOperationException("InstantPill saved capsule pool contains duplicate mystery identities.");
        }

        if (state.RemainingEffectIds.Distinct().Count() != state.RemainingEffectIds.Count)
        {
            throw new InvalidOperationException("InstantPill saved candidate-effect pool contains duplicate entries.");
        }

        if (state.CapsuleSlots.Any(slot => !PillPoolCatalog.MysteryPillIds.Contains(slot.MysteryPillId)) ||
            state.RemainingEffectIds.Any(effectId => !PillPoolCatalog.EffectPillIds.Contains(effectId)) ||
            state.CapsuleSlots.Any(slot => slot.RevealedEffectPillId != null && !PillPoolCatalog.EffectPillIds.Contains(slot.RevealedEffectPillId)))
        {
            throw new InvalidOperationException("InstantPill saved pool references a card that is not in the current catalog.");
        }
    }
}
