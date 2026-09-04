using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using InstantPill.InstantPillCode;
using InstantPill.InstantPillCode.Cards.Effect;
using MegaCrit.Sts2.Core.Entities.Cards;
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
    private static readonly ModelId SchemaV1MigrationRngId = new("INSTANTPILL", "PILL_POOL_SCHEMA_V1_MIGRATION");

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
            UpgradeSchemaV1State(player, existingState);
            ValidateExistingState(existingState);
            return existingState;
        }

        PillPoolRules.ValidateCatalog();

        Rng rng = new(player, PoolRngId);
        List<string> selectedEffects = PillPoolCatalog.EffectPillIds
            .Where(effectId => IsEligibleForCandidatePool(player, effectId))
            .ToList();
        List<string> selectedMysteries = new(PillPoolCatalog.MysteryPillIds);

        if (selectedEffects.Count == 0)
        {
            throw new InvalidOperationException("InstantPill has no eligible effect pills for its candidate pool.");
        }

        int effectivePoolSize = Math.Min(PillPoolRules.PoolSize, selectedEffects.Count);
        if (effectivePoolSize < PillPoolRules.PoolSize)
        {
            MainFile.Logger.Warn(
                $"InstantPill found only {selectedEffects.Count} eligible effect pill(s) for a target pool size of {PillPoolRules.PoolSize}. The candidate pool will use {effectivePoolSize} slot(s).",
                1);
        }

        rng.Shuffle(selectedEffects);
        rng.Shuffle(selectedMysteries);

        List<string> candidateEffects = selectedEffects.Take(effectivePoolSize).ToList();
        List<string> selectedMysteryIds = selectedMysteries.Take(effectivePoolSize).ToList();

        PillPoolState state = new()
        {
            RemainingEffectIds = candidateEffects,
            CapsuleSlots = selectedMysteryIds
                .Select((mysteryId, index) => new PillPoolSlotState
                {
                    MysteryPillId = mysteryId,
                    AssignedEffectPillId = candidateEffects[index]
                })
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
    /// Activates the effect assignment fixed for this mystery identity at run start. A null return
    /// means the supplied card is not a member of this player's capsule pool.
    /// </summary>
    public static string? RevealMysteryPill(Player player, string mysteryPillId)
    {
        return ActivateMysteryPill(player, mysteryPillId);
    }

    /// <summary>
    /// Looks up the effect fixed for this mystery identity at run start. This is intentionally
    /// non-mutating so callers can prepare card transformations before activating the reveal.
    /// </summary>
    public static string? TryGetAssignedEffectCardId(Player player, string mysteryPillId)
    {
        PillPoolState state = EnsureInitialized(player);
        PillPoolSlotState? slot = state.CapsuleSlots
            .FirstOrDefault(candidate => candidate.MysteryPillId == mysteryPillId);

        if (slot == null)
        {
            return null;
        }

        return slot.AssignedEffectPillId;
    }

    /// <summary>
    /// Looks up the mystery identity which was preassigned to an effect. Unlike normal pool APIs,
    /// this is safe for UI property getters: it never creates state, migrates saves, or advances
    /// a random number generator.
    /// </summary>
    public static string? TryGetMysteryPillIdForAssignedEffect(Player player, string effectPillId)
    {
        PillPoolState? state = State.Get(player);
        return state?.CapsuleSlots
            .FirstOrDefault(slot => slot.AssignedEffectPillId == effectPillId)
            ?.MysteryPillId;
    }

    /// <summary>
    /// Marks an existing mystery-to-effect mapping as revealed. No RNG is used here: the assigned
    /// effect was fixed when this run's pool state was initialized.
    /// </summary>
    public static string? ActivateMysteryPill(Player player, string mysteryPillId)
    {
        PillPoolState state = EnsureInitialized(player);
        PillPoolSlotState? slot = state.CapsuleSlots
            .FirstOrDefault(candidate => candidate.MysteryPillId == mysteryPillId);

        if (slot == null)
        {
            return null;
        }

        if (slot.IsRevealed)
        {
            return slot.AssignedEffectPillId;
        }

        if (!state.RemainingEffectIds.Remove(slot.AssignedEffectPillId))
        {
            throw new InvalidOperationException(
                $"InstantPill could not activate the preassigned effect for {mysteryPillId}.");
        }

        slot.IsRevealed = true;
        State.Set(player, state);

        MainFile.Logger.Info($"Activated preassigned mapping {mysteryPillId} -> {slot.AssignedEffectPillId} for player {player.NetId}.", 1);
        return slot.AssignedEffectPillId;
    }

    public static string? TryGetRevealedEffectCardId(Player player, string mysteryPillId)
    {
        PillPoolState state = EnsureInitialized(player);
        PillPoolSlotState? slot = state.CapsuleSlots
            .FirstOrDefault(candidate => candidate.MysteryPillId == mysteryPillId);
        return slot is { IsRevealed: true } ? slot.AssignedEffectPillId : null;
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

    private static bool IsEligibleForCandidatePool(Player player, string effectCardId)
    {
        CardModel canonicalCard = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), effectCardId));
        CardModel candidateCard = player.RunState.CreateCard(canonicalCard, player);

        if (candidateCard is not BaseEffectPillCard effectPill)
        {
            throw new InvalidOperationException(
                $"InstantPill effect catalog entry {effectCardId} does not create a {nameof(BaseEffectPillCard)}.");
        }

        return effectPill.Grade != BaseEffectPillCard.EffectPillGrade.Excluded;
    }

    private static void UpgradeSchemaV1State(Player player, PillPoolState state)
    {
        if (state.SchemaVersion != 1)
        {
            return;
        }

        List<PillPoolSlotState> unrevealedSlots = state.CapsuleSlots
            .Where(slot => !slot.IsRevealed)
            .ToList();

        if (unrevealedSlots.Count != state.RemainingEffectIds.Count)
        {
            throw new InvalidOperationException(
                "InstantPill schema-v1 state cannot be migrated because its unrevealed slots and remaining effects do not match.");
        }

        List<string> assignments = new(state.RemainingEffectIds);
        new Rng(player, SchemaV1MigrationRngId).Shuffle(assignments);
        for (int index = 0; index < unrevealedSlots.Count; index++)
        {
            unrevealedSlots[index].AssignedEffectPillId = assignments[index];
        }

        state.SchemaVersion = PillPoolRules.SchemaVersion;
        State.Set(player, state);
        MainFile.Logger.Info($"Migrated InstantPill pool state for player {player.NetId} from schema v1 to v2.", 1);
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

        if (state.CapsuleSlots.Select(slot => slot.AssignedEffectPillId).Distinct().Count() != state.CapsuleSlots.Count)
        {
            throw new InvalidOperationException("InstantPill saved capsule pool contains duplicate assigned effects.");
        }

        if (state.CapsuleSlots.Any(slot => !PillPoolCatalog.MysteryPillIds.Contains(slot.MysteryPillId)) ||
            state.CapsuleSlots.Any(slot => !PillPoolCatalog.EffectPillIds.Contains(slot.AssignedEffectPillId)) ||
            state.RemainingEffectIds.Any(effectId => !PillPoolCatalog.EffectPillIds.Contains(effectId)) ||
            state.RemainingEffectIds.Any(effectId => !state.CapsuleSlots.Any(slot => slot.AssignedEffectPillId == effectId)) ||
            state.CapsuleSlots.Any(slot => slot.IsRevealed && state.RemainingEffectIds.Contains(slot.AssignedEffectPillId)))
        {
            throw new InvalidOperationException("InstantPill saved pool references a card that is not in the current catalog.");
        }
    }
}
