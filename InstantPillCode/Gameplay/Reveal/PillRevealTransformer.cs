using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstantPill.InstantPillCode.Gameplay.PHD;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Reveal;

/// <summary>
/// Replaces every live instance of one revealed mystery identity with the effect each card owner
/// should materialize. The original mapping remains shared; PHD is resolved separately for each
/// affected player. Deck cards are transformed first so combat replacements can retain DeckVersion.
/// </summary>
internal static class PillRevealTransformer
{
    public static async Task<CardModel> TransformAllCopies(
        Player player,
        CardModel playedMystery,
        string assignedEffectCardId,
        string? playedEffectCardId = null)
    {
        CardModel? playedEffect = await TransformCopiesInternal(
            player,
            playedMystery.Id.Entry,
            assignedEffectCardId,
            playedMystery,
            playedEffectCardId);

        return playedEffect ?? throw new InvalidOperationException(
            $"InstantPill could not find the played mystery card {playedMystery.Id.Entry} in the combat piles.");
    }

    /// <summary>
    /// Reveals every unplayed copy of a mystery identity. PHD pickup uses this after changing the
    /// slot's normal reveal state, so deck, hand, draw and discard copies all receive the result
    /// appropriate for their own player.
    /// </summary>
    public static async Task TransformAllUnplayedCopies(
        Player player,
        string mysteryCardId,
        string assignedEffectCardId)
    {
        await TransformCopiesInternal(player, mysteryCardId, assignedEffectCardId, playedMystery: null, playedEffectCardId: null);
    }

    private static async Task<CardModel?> TransformCopiesInternal(
        Player initiatingPlayer,
        string mysteryCardId,
        string assignedEffectCardId,
        CardModel? playedMystery,
        string? playedEffectCardId)
    {
        IReadOnlyList<Player> affectedPlayers = PillPoolService.IsSharedPoolEnabled(initiatingPlayer)
            ? initiatingPlayer.RunState.Players
            : [initiatingPlayer];
        Dictionary<Player, Dictionary<CardModel, CardModel>> deckReplacementsByPlayer = [];

        foreach (Player affectedPlayer in affectedPlayers)
        {
            CardModel canonicalEffect = GetCanonicalEffectForPlayer(affectedPlayer, assignedEffectCardId);
            deckReplacementsByPlayer[affectedPlayer] = await TransformDeckCopies(
                affectedPlayer,
                mysteryCardId,
                canonicalEffect);
        }

        CardModel? playedEffect = null;
        foreach (Player affectedPlayer in affectedPlayers)
        {
            CardModel canonicalEffect = GetCanonicalEffectForPlayer(affectedPlayer, assignedEffectCardId);
            CardModel canonicalPlayedEffect = ReferenceEquals(affectedPlayer, initiatingPlayer) && playedEffectCardId != null
                ? GetCanonicalCard(playedEffectCardId)
                : canonicalEffect;
            CardModel? transformedPlayedCard = await TransformCombatCopies(
                affectedPlayer,
                playedMystery,
                mysteryCardId,
                canonicalEffect,
                canonicalPlayedEffect,
                deckReplacementsByPlayer[affectedPlayer]);
            if (ReferenceEquals(affectedPlayer, initiatingPlayer))
            {
                playedEffect = transformedPlayedCard;
            }
        }

        return playedEffect;
    }

    private static CardModel GetCanonicalEffectForPlayer(Player player, string assignedEffectCardId) =>
        GetCanonicalCard(PillPhdResolver.ResolveEffectCardId(player, assignedEffectCardId));

    private static CardModel GetCanonicalCard(string cardId) => ModelDb.GetById<CardModel>(
        new ModelId(ModelId.SlugifyCategory<CardModel>(), cardId));

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
        CardModel? playedMystery,
        string mysteryCardId,
        CardModel canonicalEffect,
        CardModel canonicalPlayedEffect,
        IReadOnlyDictionary<CardModel, CardModel> deckReplacements)
    {
        PlayerCombatState? playerCombatState = player.PlayerCombatState;
        if (playerCombatState == null)
        {
            if (playedMystery != null && ReferenceEquals(player, playedMystery.Owner))
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
