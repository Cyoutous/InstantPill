using System;
using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using InstantPill.InstantPillCode.Configuration;
using InstantPill.InstantPillCode.Gameplay.Pools;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>
/// Generates the two independent capsule-reward channels without touching vanilla potion odds
/// or the RNG stream used to establish capsule/effect mappings.
/// </summary>
public static class PillRewardService
{
    private static readonly ModelId IndependentRngId = new("INSTANTPILL", "PILL_REWARD_INDEPENDENT");
    private static readonly ModelId ReplacementRngId = new("INSTANTPILL", "PILL_REWARD_REPLACEMENT");
    private static readonly ModelId ChoiceRngId = new("INSTANTPILL", "PILL_REWARD_CHOICES");

    public static SavedSpireField<Player, PillRewardRunState> State { get; } =
        new(_ => null, "instant_pill_reward_odds");

    public static PillRewardRunState EnsureInitialized(Player player)
    {
        PillRewardRunState? existingState = State.Get(player);
        if (existingState != null)
        {
            ValidateExistingState(existingState);
            return existingState;
        }

        PillRewardRulesSnapshot rules = PillRewardSettings.CreateRulesForNewRun(PillPoolRules.PoolSize);
        PillRewardRunState state = new()
        {
            Rules = rules,
            IndependentCurrentOdds = rules.IndependentDrop.BaseOdds,
            ReplacementCurrentOdds = rules.PotionReplacement.BaseOdds
        };
        State.Set(player, state);
        MainFile.Logger.Info(
            $"Initialized InstantPill reward odds for player {player.NetId}: independent={state.IndependentCurrentOdds:P0}, replacement={state.ReplacementCurrentOdds:P0}.",
            1);
        return state;
    }

    /// <summary>
    /// Applies independent generation attempts, guaranteed independent reward floors, and the
    /// separate potion-replacement channel to an already-generated combat reward list.
    /// </summary>
    public static void AddCombatRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (room is not CombatRoom || !IsSupportedRoomType(room.RoomType))
        {
            return;
        }

        PillRewardRunState state = EnsureInitialized(player);
        if (!IsEnabledForRoom(state.Rules, room.RoomType))
        {
            return;
        }

        HashSet<string> offeredCardIds = new(StringComparer.Ordinal);
        int successfulIndependentRolls = 0;
        int independentAttemptCount = GetIndependentAttemptCount(state.Rules, room.RoomType);
        for (int attemptIndex = 0; attemptIndex < independentAttemptCount; attemptIndex++)
        {
            if (RollIndependent(player, state, room.RoomType))
            {
                successfulIndependentRolls++;
            }
        }

        int independentRewardCount = Math.Max(
            successfulIndependentRolls,
            GetGuaranteedRewardCount(state.Rules, room.RoomType));
        MainFile.Logger.Info(
            $"InstantPill {room.RoomType} capsule rewards: attempts={independentAttemptCount}, successes={successfulIndependentRolls}, guaranteed={GetGuaranteedRewardCount(state.Rules, room.RoomType)}, panels={independentRewardCount}.",
            1);
        for (int rewardIndex = 0; rewardIndex < independentRewardCount; rewardIndex++)
        {
            PillCardReward? reward = CreatePillReward(player, state, room.RoomType, offeredCardIds);
            if (reward != null)
            {
                InsertIndependentReward(rewards, reward);
            }
        }

        int potionIndex = rewards.FindIndex(reward => reward is PotionReward);
        if (potionIndex >= 0 && RollReplacement(player, state, room.RoomType))
        {
            PillCardReward? replacement = CreatePillReward(player, state, room.RoomType, offeredCardIds);
            if (replacement != null)
            {
                rewards[potionIndex] = replacement;
            }
        }

        State.Set(player, state);
    }

    private static bool RollIndependent(Player player, PillRewardRunState state, RoomType roomType)
    {
        int counter = state.IndependentRollCounter;
        float currentOdds = state.IndependentCurrentOdds;
        bool result = Roll(
            player,
            IndependentRngId,
            ref counter,
            ref currentOdds,
            state.Rules.IndependentDrop,
            roomType);
        state.IndependentRollCounter = counter;
        state.IndependentCurrentOdds = currentOdds;
        MainFile.Logger.Info(
            $"InstantPill independent reward roll for player {player.NetId}: {result}, next odds={state.IndependentCurrentOdds:P0}.",
            1);
        return result;
    }

    private static bool RollReplacement(Player player, PillRewardRunState state, RoomType roomType)
    {
        int counter = state.ReplacementRollCounter;
        float currentOdds = state.ReplacementCurrentOdds;
        bool result = Roll(
            player,
            ReplacementRngId,
            ref counter,
            ref currentOdds,
            state.Rules.PotionReplacement,
            roomType);
        state.ReplacementRollCounter = counter;
        state.ReplacementCurrentOdds = currentOdds;
        MainFile.Logger.Info(
            $"InstantPill potion-replacement roll for player {player.NetId}: {result}, next odds={state.ReplacementCurrentOdds:P0}.",
            1);
        return result;
    }

    private static bool Roll(
        Player player,
        ModelId rngId,
        ref int counter,
        ref float currentOdds,
        PillOddsRules rules,
        RoomType roomType)
    {
        if (counter < 0)
        {
            throw new InvalidOperationException("InstantPill reward RNG counter is invalid.");
        }

        float effectiveOdds = Math.Clamp(currentOdds + GetRoomBonus(rules, roomType), rules.MinimumOdds, rules.MaximumOdds);
        Rng rng = new(player, rngId, (ulong)counter++);
        bool succeeded = rng.NextFloat() < effectiveOdds;
        currentOdds = Math.Clamp(
            currentOdds + (succeeded ? -rules.SuccessDecrease : rules.FailureIncrease),
            rules.MinimumOdds,
            rules.MaximumOdds);
        return succeeded;
    }

    private static PillCardReward? CreatePillReward(
        Player player,
        PillRewardRunState state,
        RoomType roomType,
        ISet<string> offeredCardIds)
    {
        int choiceCount = GetChoiceCount(state.Rules, roomType);
        IReadOnlyList<string> cardIds = SelectCapsuleCardIds(player, state, choiceCount, offeredCardIds);
        if (cardIds.Count == 0)
        {
            MainFile.Logger.Warn("InstantPill rolled a capsule reward but found no valid capsule choices.");
            return null;
        }

        List<CardModel> cards = cardIds
            .Select(cardId => ModelDb.GetById<CardModel>(
                new ModelId(ModelId.SlugifyCategory<CardModel>(), cardId)))
            .Select(canonicalCard => player.RunState.CreateCard(canonicalCard, player))
            .ToList();
        foreach (string cardId in cardIds)
        {
            offeredCardIds.Add(cardId);
        }

        return new PillCardReward(cards, player, CardCreationOptions.ForRoom(player, roomType));
    }

    private static IReadOnlyList<string> SelectCapsuleCardIds(
        Player player,
        PillRewardRunState state,
        int choiceCount,
        ISet<string> offeredCardIds)
    {
        PillPoolState poolState = PillPoolService.EnsureInitialized(player);
        IEnumerable<string> currentCardIds = poolState.CapsuleSlots
            .Select(slot => slot.CurrentCardId)
            .Distinct(StringComparer.Ordinal);
        if (state.Rules.PreventDuplicateCapsuleOptionsWithinCombat)
        {
            currentCardIds = currentCardIds.Where(cardId => !offeredCardIds.Contains(cardId));
        }

        List<string> candidates = currentCardIds.ToList();
        if (state.ChoiceRollCounter < 0)
        {
            throw new InvalidOperationException("InstantPill reward choice RNG counter is invalid.");
        }

        new Rng(player, ChoiceRngId, (ulong)state.ChoiceRollCounter++).Shuffle(candidates);
        return candidates.Take(Math.Min(choiceCount, candidates.Count)).ToArray();
    }

    private static void InsertIndependentReward(List<Reward> rewards, Reward pillReward)
    {
        int firstCardReward = rewards.FindIndex(reward => reward is CardReward);
        if (firstCardReward < 0)
        {
            rewards.Add(pillReward);
            return;
        }

        rewards.Insert(firstCardReward, pillReward);
    }

    private static bool IsSupportedRoomType(RoomType roomType) =>
        roomType is RoomType.Monster or RoomType.Elite or RoomType.Boss;

    private static bool IsEnabledForRoom(PillRewardRulesSnapshot rules, RoomType roomType) => roomType switch
    {
        RoomType.Monster => rules.EnableNormalRewards,
        RoomType.Elite => rules.EnableEliteRewards,
        RoomType.Boss => rules.EnableBossRewards,
        _ => false
    };

    private static int GetChoiceCount(PillRewardRulesSnapshot rules, RoomType roomType) => roomType switch
    {
        RoomType.Monster => rules.NormalChoiceCount,
        RoomType.Elite => rules.EliteChoiceCount,
        RoomType.Boss => rules.BossChoiceCount,
        _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null)
    };

    private static int GetIndependentAttemptCount(PillRewardRulesSnapshot rules, RoomType roomType) => roomType switch
    {
        RoomType.Monster => rules.NormalGenerationAttempts,
        RoomType.Elite => rules.EliteGenerationAttempts,
        RoomType.Boss => rules.BossGenerationAttempts,
        _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null)
    };

    private static int GetGuaranteedRewardCount(PillRewardRulesSnapshot rules, RoomType roomType) => roomType switch
    {
        RoomType.Monster => rules.NormalGuaranteedRewardCount,
        RoomType.Elite => rules.EliteGuaranteedRewardCount,
        RoomType.Boss => rules.BossGuaranteedRewardCount,
        _ => throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null)
    };

    private static float GetRoomBonus(PillOddsRules rules, RoomType roomType) => roomType switch
    {
        RoomType.Elite => rules.EliteBonus,
        RoomType.Boss => rules.BossBonus,
        _ => 0f
    };

    private static void ValidateExistingState(PillRewardRunState state)
    {
        if (state.SchemaVersion != PillRewardRunState.CurrentSchemaVersion)
        {
            throw new InvalidOperationException($"Unsupported InstantPill reward-state schema {state.SchemaVersion}.");
        }

        if (state.IndependentRollCounter < 0 || state.ReplacementRollCounter < 0 || state.ChoiceRollCounter < 0)
        {
            throw new InvalidOperationException("InstantPill saved reward RNG counter is invalid.");
        }

        state.Rules.Normalize(PillPoolRules.PoolSize);
        state.IndependentCurrentOdds = Math.Clamp(
            state.IndependentCurrentOdds,
            state.Rules.IndependentDrop.MinimumOdds,
            state.Rules.IndependentDrop.MaximumOdds);
        state.ReplacementCurrentOdds = Math.Clamp(
            state.ReplacementCurrentOdds,
            state.Rules.PotionReplacement.MinimumOdds,
            state.Rules.PotionReplacement.MaximumOdds);
    }
}
