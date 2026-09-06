using System;
using InstantPill.InstantPillCode.Gameplay.Pools;
using InstantPill.InstantPillCode.Gameplay.Rewards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;

namespace InstantPill.InstantPillCode.Gameplay.StartingDeck;

/// <summary>
/// Adds configured mystery-pill copies after a new run has established its fixed capsule/effect
/// mapping. This never runs for a loaded run.
/// </summary>
internal static class InitialMysteryPillService
{
    private static readonly ModelId StartingPillRngId = new("INSTANTPILL", "STARTING_MYSTERY_PILLS");

    public static void GrantForNewRun(Player player)
    {
        PillRewardRunState rewardState = PillRewardService.EnsureInitialized(player);
        int count = rewardState.Rules.InitialMysteryPillCount;
        if (count <= 0)
        {
            return;
        }

        PillPoolState poolState = PillPoolService.EnsureInitialized(player);
        if (poolState.CapsuleSlots.Count == 0)
        {
            throw new InvalidOperationException("InstantPill cannot grant starting capsules because the capsule pool is empty.");
        }

        // Sampling is intentionally with replacement. There are only thirteen run-local mystery
        // identities, but the user-facing setting permits up to one thousand starting copies.
        Rng rng = new(player, StartingPillRngId);
        for (int index = 0; index < count; index++)
        {
            string mysteryId = poolState.CapsuleSlots[rng.NextInt(poolState.CapsuleSlots.Count)].MysteryPillId;
            CardModel canonicalCard = ModelDb.GetById<CardModel>(
                new ModelId(ModelId.SlugifyCategory<CardModel>(), mysteryId));
            CardModel card = player.RunState.CreateCard(canonicalCard, player);
            card.FloorAddedToDeck = 1;
            player.Deck.AddInternal(card);
        }

        MainFile.Logger.Info(
            $"Added {count} configured starting mystery pill(s) to player {player.NetId}'s deck.",
            1);
    }
}
