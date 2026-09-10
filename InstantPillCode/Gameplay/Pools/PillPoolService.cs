using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using InstantPill.InstantPillCode;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Gameplay.PHD;
using InstantPill.InstantPillCode.Gameplay.Rewards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

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
    /// Holds the run-start mode snapshot and, when enabled, the one pool shared by every player
    /// in a multiplayer run. The existing <see cref="State"/> remains the independent-pool
    /// storage used by single-player and by multiplayer runs with the setting disabled.
    /// </summary>
    public static SavedSpireField<IRunState, PillPoolSessionState> SessionState { get; } =
        new(_ => null, "instant_pill_pool_session");

    /// <summary>
    /// Snapshots the host-authoritative multiplayer decision at run creation. Once stored, this
    /// state—not a later settings menu change—controls how every pool lookup is routed.
    /// </summary>
    public static void InitializeRunState(IRunState runState)
    {
        if (SessionState.Get(runState) != null)
        {
            return;
        }

        bool sharedPoolEnabled = PillMultiplayerRulesService.IsSharedPillPoolEnabled(runState);
        SessionState.Set(runState, new PillPoolSessionState
        {
            SharedPoolEnabled = sharedPoolEnabled
        });

        MainFile.Logger.Info(
            $"Initialized InstantPill pool session: shared multiplayer pool = {sharedPoolEnabled}.",
            1);
    }

    /// <summary>Returns whether this player's run uses the shared capsule-pool state.</summary>
    public static bool IsSharedPoolEnabled(Player player) =>
        GetOrCreateSessionState(player.RunState).SharedPoolEnabled;

    /// <summary>
    /// Returns whether an effect-card catalogue entry may participate in gameplay randomization.
    /// Grade metadata is static model data, so this lookup intentionally does not create a card
    /// instance or consume any run RNG.
    /// </summary>
    public static bool IsEligibleEffectPill(string effectCardId)
    {
        CardModel canonicalCard = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), effectCardId));
        if (canonicalCard is not BaseEffectPillCard effectPill)
        {
            throw new InvalidOperationException(
                $"InstantPill effect catalog entry {effectCardId} is not a {nameof(BaseEffectPillCard)}.");
        }

        return effectPill.Grade != BaseEffectPillCard.EffectPillGrade.Excluded;
    }

    /// <summary>
    /// Creates this player's pools only when no saved state exists. A restored state is validated,
    /// never rerolled.
    /// </summary>
    public static PillPoolState EnsureInitialized(Player player)
    {
        PillPoolSessionState session = GetOrCreateSessionState(player.RunState);
        if (session.SharedPoolEnabled)
        {
            return EnsureSharedInitialized(player.RunState, session);
        }

        PillPoolState? existingState = State.Get(player);
        if (existingState != null)
        {
            UpgradeSchemaV1State(player, existingState);
            ValidateExistingState(existingState);
            return existingState;
        }

        PillPoolState state = CreatePoolState(player);
        State.Set(player, state);
        MainFile.Logger.Info(
            $"Initialized InstantPill pools for player {player.NetId}: {state.CapsuleSlots.Count} capsule slots and {state.RemainingEffectIds.Count} candidate effects.",
            1);
        return state;
    }

    private static PillPoolState EnsureSharedInitialized(IRunState runState, PillPoolSessionState session)
    {
        Player rngOwner = GetSharedRngOwner(runState);
        if (session.HasSharedPool)
        {
            UpgradeSchemaV1State(rngOwner, session.SharedPool, isSharedPool: true);
            ValidateExistingState(session.SharedPool);
            return session.SharedPool;
        }

        session.SharedPool = CreatePoolState(rngOwner);
        session.HasSharedPool = true;
        SessionState.Set(runState, session);
        MainFile.Logger.Info(
            $"Initialized shared InstantPill pool: {session.SharedPool.CapsuleSlots.Count} capsule slots and {session.SharedPool.RemainingEffectIds.Count} candidate effects.",
            1);
        return session.SharedPool;
    }

    private static PillPoolState CreatePoolState(Player rngOwner)
    {
        PillPoolRules.ValidateCatalog();

        Rng rng = new(rngOwner, PoolRngId);
        List<EffectCandidate> eligibleEffects = PillPoolCatalog.EffectPillIds
            .Select(effectId => CreateEffectCandidate(rngOwner, effectId))
            .Where(candidate => candidate.Grade != BaseEffectPillCard.EffectPillGrade.Excluded)
            .ToList();
        List<string> selectedMysteries = new(PillPoolCatalog.MysteryPillIds);

        if (eligibleEffects.Count == 0)
        {
            throw new InvalidOperationException("InstantPill has no eligible effect pills for its candidate pool.");
        }

        int effectivePoolSize = Math.Min(PillPoolRules.PoolSize, eligibleEffects.Count);
        if (effectivePoolSize < PillPoolRules.PoolSize)
        {
            MainFile.Logger.Warn(
                $"InstantPill found only {eligibleEffects.Count} eligible effect pill(s) for a target pool size of {PillPoolRules.PoolSize}. The candidate pool will use {effectivePoolSize} slot(s).",
                1);
        }

        List<string> candidateEffects = SelectCandidateEffects(eligibleEffects, effectivePoolSize, rng);
        rng.Shuffle(selectedMysteries);

        List<string> selectedMysteryIds = selectedMysteries.Take(effectivePoolSize).ToList();

        return new PillPoolState
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
    }

    /// <summary>
    /// Randomly selects a capsule-pool slot without removing it. The original slot is selected
    /// first; its returned card identity is then materialized for this player, including PHD.
    /// </summary>
    public static string RollCapsuleCardId(Player player)
    {
        return RollCapsuleCardIdExcluding(player, excludedCardId: null);
    }

    /// <summary>
    /// Selects one original assigned effect from this run's capsule slots, then materializes it
    /// for the supplied player. PHD uses this on pickup before it reveals the slots, so the card
    /// awarded is an identified effect pill rather than an additional mystery pill.
    /// </summary>
    public static string RollAssignedEffectCardIdForPlayer(Player player)
    {
        PillPoolState state = EnsureInitialized(player);
        if (state.CapsuleSlots.Count == 0)
        {
            throw new InvalidOperationException("InstantPill cannot roll an assigned effect from an empty capsule pool.");
        }

        Rng rng = NextPoolRng(GetPoolRngOwner(player), state);
        PillPoolSlotState slot = state.CapsuleSlots[rng.NextInt(state.CapsuleSlots.Count)];
        PersistPoolState(player, state);
        return PillPhdResolver.ResolveEffectCardId(player, slot.AssignedEffectPillId);
    }

    /// <summary>
    /// Randomly selects a capsule-pool slot while excluding one original current identity. This
    /// still rolls slots rather than distinct identities, preserving normal slot weighting before
    /// PHD materializes the selected result for its owner.
    /// </summary>
    public static string RollCapsuleCardIdExcluding(Player player, string? excludedCardId)
    {
        PillPoolState state = EnsureInitialized(player);
        List<PillPoolSlotState> candidates = state.CapsuleSlots
            .Where(slot => !string.Equals(slot.CurrentCardId, excludedCardId, StringComparison.Ordinal))
            .ToList();
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException("InstantPill cannot roll a capsule from the available capsule pool.");
        }

        Rng rng = NextPoolRng(GetPoolRngOwner(player), state);
        PillPoolSlotState slot = candidates[rng.NextInt(candidates.Count)];
        PersistPoolState(player, state);
        return ResolveSlotCardIdForPlayer(player, slot);
    }

    /// <summary>
    /// Resolves a slot's current card identity for one player. An unrevealed slot remains its
    /// mystery pill; a revealed slot retains its original assigned effect in save data but may
    /// materialize as that effect's PHD replacement.
    /// </summary>
    public static string ResolveSlotCardIdForPlayer(Player player, PillPoolSlotState slot) =>
        slot.IsRevealed
            ? PillPhdResolver.ResolveEffectCardId(player, slot.AssignedEffectPillId)
            : slot.MysteryPillId;

    /// <summary>
    /// Applies PHD to a known capsule identity without mutating any pool state. This is used by
    /// pathways such as Vurp which may begin from a previously materialized capsule ID.
    /// </summary>
    public static string ResolveCapsuleCardIdForPlayer(Player player, string cardId)
    {
        if (PillPoolCatalog.EffectPillIds.Contains(cardId, StringComparer.Ordinal))
        {
            return PillPhdResolver.ResolveEffectCardId(player, cardId);
        }

        return cardId;
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
        PillPoolState? state = TryGetExistingPoolState(player);
        return state?.CapsuleSlots
            .FirstOrDefault(slot => slot.AssignedEffectPillId == effectPillId)
            ?.MysteryPillId;
    }

    /// <summary>
    /// Returns the first selected source slot whose PHD replacement is <paramref name="effectPillId"/>,
    /// but only when the requested effect was not selected itself. This derives a display-only
    /// portrait alias from immutable pool data; it neither initializes a pool nor changes a save.
    /// </summary>
    public static string? TryGetPhdInheritedMysteryPillId(Player player, string effectPillId)
    {
        PillPoolState? state = TryGetExistingPoolState(player);
        if (state == null || state.CapsuleSlots.Any(slot => slot.AssignedEffectPillId == effectPillId))
        {
            return null;
        }

        foreach (PillPoolSlotState slot in state.CapsuleSlots)
        {
            CardModel canonicalCard = ModelDb.GetById<CardModel>(
                new ModelId(ModelId.SlugifyCategory<CardModel>(), slot.AssignedEffectPillId));
            if (canonicalCard is BaseEffectPillCard effectPill &&
                string.Equals(effectPill.PhdReplacementCardId, effectPillId, StringComparison.Ordinal))
            {
                // CapsuleSlots retain the selected candidate order, making this deterministic
                // when multiple sources someday share one PHD replacement target.
                return slot.MysteryPillId;
            }
        }

        return null;
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
        PersistPoolState(player, state);

        MainFile.Logger.Info(
            $"Activated preassigned mapping {mysteryPillId} -> {slot.AssignedEffectPillId} for {(IsSharedPoolEnabled(player) ? "shared pool" : $"player {player.NetId}")}.",
            1);
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

    private static List<string> SelectCandidateEffects(
        List<EffectCandidate> eligibleEffects,
        int targetCount,
        Rng rng)
    {
        List<EffectCandidate> remainingEffects = new(eligibleEffects);
        List<string> selectedEffectIds = new(targetCount);

        foreach ((BaseEffectPillCard.EffectPillGrade grade, int quota) in PillPoolRules.PriorityGradeQuotas)
        {
            if (selectedEffectIds.Count >= targetCount)
            {
                break;
            }

            List<EffectCandidate> candidatesAtGrade = remainingEffects
                .Where(candidate => candidate.Grade == grade)
                .ToList();
            rng.Shuffle(candidatesAtGrade);

            int drawCount = Math.Min(quota, targetCount - selectedEffectIds.Count);
            foreach (EffectCandidate selected in candidatesAtGrade.Take(drawCount))
            {
                selectedEffectIds.Add(selected.Id);
                remainingEffects.Remove(selected);
            }
        }

        rng.Shuffle(remainingEffects);
        selectedEffectIds.AddRange(remainingEffects
            .Take(Math.Max(0, targetCount - selectedEffectIds.Count))
            .Select(candidate => candidate.Id));

        return selectedEffectIds;
    }

    private static EffectCandidate CreateEffectCandidate(Player player, string effectCardId)
    {
        CardModel canonicalCard = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), effectCardId));
        CardModel candidateCard = player.RunState.CreateCard(canonicalCard, player);

        if (candidateCard is not BaseEffectPillCard effectPill)
        {
            throw new InvalidOperationException(
                $"InstantPill effect catalog entry {effectCardId} does not create a {nameof(BaseEffectPillCard)}.");
        }

        return new EffectCandidate(effectCardId, effectPill.Grade);
    }

    private readonly record struct EffectCandidate(
        string Id,
        BaseEffectPillCard.EffectPillGrade Grade);

    private static void UpgradeSchemaV1State(Player player, PillPoolState state, bool isSharedPool = false)
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
        if (isSharedPool)
        {
            PillPoolSessionState session = GetOrCreateSessionState(player.RunState);
            session.SharedPool = state;
            session.HasSharedPool = true;
            SessionState.Set(player.RunState, session);
        }
        else
        {
            State.Set(player, state);
        }

        MainFile.Logger.Info(
            $"Migrated InstantPill {(isSharedPool ? "shared" : $"player {player.NetId}")} pool state from schema v1 to v2.",
            1);
    }

    private static PillPoolSessionState GetOrCreateSessionState(IRunState runState)
    {
        PillPoolSessionState? session = SessionState.Get(runState);
        if (session != null)
        {
            return session;
        }

        InitializeRunState(runState);
        return SessionState.Get(runState)
            ?? throw new InvalidOperationException("InstantPill could not initialize its run-level pool session.");
    }

    private static PillPoolState? TryGetExistingPoolState(Player player)
    {
        PillPoolSessionState? session = SessionState.Get(player.RunState);
        if (session is { SharedPoolEnabled: true })
        {
            return session.HasSharedPool ? session.SharedPool : null;
        }

        return State.Get(player);
    }

    private static void PersistPoolState(Player player, PillPoolState state)
    {
        PillPoolSessionState session = GetOrCreateSessionState(player.RunState);
        if (session.SharedPoolEnabled)
        {
            session.SharedPool = state;
            session.HasSharedPool = true;
            SessionState.Set(player.RunState, session);
            return;
        }

        State.Set(player, state);
    }

    private static Player GetPoolRngOwner(Player player) =>
        IsSharedPoolEnabled(player) ? GetSharedRngOwner(player.RunState) : player;

    private static Player GetSharedRngOwner(IRunState runState) => runState.Players
        .OrderBy(player => player.NetId)
        .FirstOrDefault()
        ?? throw new InvalidOperationException("InstantPill cannot initialize a shared pool without a player.");

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
