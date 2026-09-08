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
/// When the run uses a shared pool, this covers every player in the run; otherwise it remains
/// limited to the owner of the revealed card. Deck cards are transformed first so combat
/// replacements can retain the appropriate DeckVersion.
/// </summary>
internal static class PillRevealTransformer
{
    public static async Task<CardModel> TransformAllCopies(
        Player player,
        CardModel playedMystery,
        string effectCardId,
        string? playedEffectCardId = null)
    {
        CardModel canonicalEffect = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), effectCardId));
        CardModel canonicalPlayedEffect = playedEffectCardId == null
            ? canonicalEffect
            : ModelDb.GetById<CardModel>(
                new ModelId(ModelId.SlugifyCategory<CardModel>(), playedEffectCardId));
        string mysteryCardId = playedMystery.Id.Entry;

        IReadOnlyList<Player> affectedPlayers = PillPoolService.IsSharedPoolEnabled(player)
            ? player.RunState.Players
            : [player];
        Dictionary<Player, Dictionary<CardModel, CardModel>> deckReplacementsByPlayer = [];

        foreach (Player affectedPlayer in affectedPlayers)
        {
            deckReplacementsByPlayer[affectedPlayer] = await TransformDeckCopies(
                affectedPlayer,
                mysteryCardId,
                canonicalEffect);
        }

        CardModel? playedEffect = null;
        foreach (Player affectedPlayer in affectedPlayers)
        {
            CardModel? transformedPlayedCard = await TransformCombatCopies(
                affectedPlayer,
                playedMystery,
                mysteryCardId,
                canonicalEffect,
                canonicalPlayedEffect,
                deckReplacementsByPlayer[affectedPlayer]);
            if (ReferenceEquals(affectedPlayer, player))
            {
                playedEffect = transformedPlayedCard;
            }
        }

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
        CardModel canonicalPlayedEffect,
        IReadOnlyDictionary<CardModel, CardModel> deckReplacements)
    {
        PlayerCombatState? playerCombatState = player.PlayerCombatState;
        if (playerCombatState == null)
        {
            if (ReferenceEquals(player, playedMystery.Owner))
            {
                throw new InvalidOperationException("InstantPill cannot reveal a mystery pill outside combat.");
            }

            // A remote player can have no combat state after disconnecting or being removed
            // from a combat. Their permanent deck was already updated above.
            return null;
        }

        CardModel[] combatCopies = playerCombatState.AllCards
            .Where(card => card.Id.Entry == mysteryCardId)
            .ToArray();
        CardModel? playedEffect = null;

        foreach (CardModel combatCopy in combatCopies)
        {
            ICardScope scope = combatCopy.CardScope
                ?? throw new InvalidOperationException($"InstantPill card {combatCopy.Id.Entry} has no card scope.");
            CardModel replacement = scope.CreateCard(
                ReferenceEquals(combatCopy, playedMystery) ? canonicalPlayedEffect : canonicalEffect,
                player);

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
