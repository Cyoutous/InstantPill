using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Gameplay.Pools;
using InstantPill.InstantPillCode.Gameplay.Reveal;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.PHD;

/// <summary>
/// Applies PHD's one-time pickup behaviour. It reveals the normal, immutable pool assignments
/// and then materializes each card for its owner; it never rewrites an assigned-effect ID.
/// </summary>
public static class PhdPillConversionService
{
    public static async Task OnPhdObtained(Player player)
    {
        if (!PillPhdResolver.HasPhd(player))
        {
            throw new InvalidOperationException("InstantPill attempted PHD conversion for a player without PHD.");
        }

        // Existing identified pills belong only to the new PHD owner. Do this before revealing
        // mysteries so a replacement whose own metadata has a replacement cannot form a chain.
        await TransformOwnedIdentifiedEffectPills(player);

        PillPoolState state = PillPoolService.EnsureInitialized(player);
        PillPoolSlotState[] unrevealedSlots = state.CapsuleSlots
            .Where(slot => !slot.IsRevealed)
            .ToArray();

        foreach (PillPoolSlotState slot in unrevealedSlots)
        {
            string? assignedEffectId = PillPoolService.ActivateMysteryPill(player, slot.MysteryPillId);
            if (assignedEffectId == null)
            {
                throw new InvalidOperationException(
                    $"InstantPill could not reveal PHD slot {slot.MysteryPillId}.");
            }

            // In a shared-pool run this reveals the slot for every player, but the transformer
            // resolves PHD independently per card owner.
            await PillRevealTransformer.TransformAllUnplayedCopies(
                player,
                slot.MysteryPillId,
                assignedEffectId);
        }

        MainFile.Logger.Info(
            $"Applied PHD pill conversion for player {player.NetId}; revealed {unrevealedSlots.Length} mystery slot(s).",
            1);
    }

    private static async Task TransformOwnedIdentifiedEffectPills(Player player)
    {
        Dictionary<CardModel, CardModel> deckReplacements = [];
        CardModel[] deckCopies = player.Deck.Cards
            .Where(card => NeedsReplacement(player, card))
            .ToArray();

        foreach (CardModel deckCopy in deckCopies)
        {
            CardModel replacement = player.RunState.CreateCard(
                GetCanonicalReplacement(player, deckCopy),
                player);
            CardPileAddResult? result = await CardCmd.Transform(
                deckCopy,
                replacement,
                CardPreviewStyle.None);
            if (result is not { success: true })
            {
                throw new InvalidOperationException($"InstantPill failed to PHD-transform deck card {deckCopy.Id.Entry}.");
            }

            deckReplacements.Add(deckCopy, result.Value.cardAdded);
        }

        PlayerCombatState? combatState = player.PlayerCombatState;
        if (combatState == null)
        {
            return;
        }

        CardModel[] combatCopies = combatState.AllCards
            .Where(card => NeedsReplacement(player, card))
            .ToArray();
        foreach (CardModel combatCopy in combatCopies)
        {
            ICardScope scope = combatCopy.CardScope
                ?? throw new InvalidOperationException($"InstantPill card {combatCopy.Id.Entry} has no card scope.");
            CardModel replacement = scope.CreateCard(GetCanonicalReplacement(player, combatCopy), player);
            if (combatCopy.DeckVersion != null &&
                deckReplacements.TryGetValue(combatCopy.DeckVersion, out CardModel? deckReplacement))
            {
                replacement.DeckVersion = deckReplacement;
            }

            CardPileAddResult? result = await CardCmd.Transform(
                combatCopy,
                replacement,
                CardPreviewStyle.None);
            if (result is not { success: true })
            {
                throw new InvalidOperationException($"InstantPill failed to PHD-transform combat card {combatCopy.Id.Entry}.");
            }
        }
    }

    private static bool NeedsReplacement(Player player, CardModel card) =>
        card is BaseEffectPillCard &&
        !string.Equals(
            card.Id.Entry,
            PillPhdResolver.ResolveEffectCardId(player, card.Id.Entry),
            StringComparison.Ordinal);

    private static CardModel GetCanonicalReplacement(Player player, CardModel card)
    {
        string replacementId = PillPhdResolver.ResolveEffectCardId(player, card.Id.Entry);
        return ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), replacementId));
    }
}
