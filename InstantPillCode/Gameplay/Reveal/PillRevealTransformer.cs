using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Reveal;

/// <summary>
/// Replaces every live instance of one revealed mystery identity with its preassigned effect card.
/// Deck cards are transformed first so combat replacements can retain the appropriate DeckVersion.
/// </summary>
internal static class PillRevealTransformer
{
    public static async Task<CardModel> TransformAllCopies(
        Player player,
        CardModel playedMystery,
        string effectCardId)
    {
        CardModel canonicalEffect = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), effectCardId));
        string mysteryCardId = playedMystery.Id.Entry;

        Dictionary<CardModel, CardModel> deckReplacements = await TransformDeckCopies(
            player,
            mysteryCardId,
            canonicalEffect);
        CardModel? playedEffect = await TransformCombatCopies(
            player,
            playedMystery,
            mysteryCardId,
            canonicalEffect,
            deckReplacements);

        return playedEffect ?? throw new InvalidOperationException(
            $"InstantPill could not find the played mystery card {mysteryCardId} in the combat piles.");
    }

    private static async Task<Dictionary<CardModel, CardModel>> TransformDeckCopies(
        Player player,
        string mysteryCardId,
        CardModel canonicalEffect)
    {
        Dictionary<CardModel, CardModel> replacements = [];
        CardModel[] deckCopies = player.Deck.Cards
            .Where(card => card.Id.Entry == mysteryCardId)
            .ToArray();

        foreach (CardModel deckCopy in deckCopies)
        {
            CardModel replacement = player.RunState.CreateCard(canonicalEffect, player);
            CardPileAddResult? result = await CardCmd.Transform(
                deckCopy,
                replacement,
                CardPreviewStyle.None);

            if (result is not { success: true })
            {
                throw new InvalidOperationException($"InstantPill failed to transform deck card {deckCopy.Id.Entry}.");
            }

            replacements.Add(deckCopy, result.Value.cardAdded);
        }

        return replacements;
    }

    private static async Task<CardModel?> TransformCombatCopies(
        Player player,
        CardModel playedMystery,
        string mysteryCardId,
        CardModel canonicalEffect,
        IReadOnlyDictionary<CardModel, CardModel> deckReplacements)
    {
        PlayerCombatState? playerCombatState = player.PlayerCombatState;
        if (playerCombatState == null)
        {
            throw new InvalidOperationException("InstantPill cannot reveal a mystery pill outside combat.");
        }

        CardModel[] combatCopies = playerCombatState.AllCards
            .Where(card => card.Id.Entry == mysteryCardId)
            .ToArray();
        CardModel? playedEffect = null;

        foreach (CardModel combatCopy in combatCopies)
        {
            ICardScope scope = combatCopy.CardScope
                ?? throw new InvalidOperationException($"InstantPill card {combatCopy.Id.Entry} has no card scope.");
            CardModel replacement = scope.CreateCard(canonicalEffect, player);

            if (combatCopy.DeckVersion != null && deckReplacements.TryGetValue(combatCopy.DeckVersion, out CardModel? deckReplacement))
            {
                replacement.DeckVersion = deckReplacement;
            }

            CardPileAddResult? result = await CardCmd.Transform(
                combatCopy,
                replacement,
                CardPreviewStyle.None);
            if (result is not { success: true })
            {
                throw new InvalidOperationException($"InstantPill failed to transform combat card {combatCopy.Id.Entry}.");
            }

            if (ReferenceEquals(combatCopy, playedMystery))
            {
                playedEffect = result.Value.cardAdded;
            }
        }

        return playedEffect;
    }
}
