using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Gameplay;

/// <summary>
/// Implements the non-display behaviour owned by the InstantPill Capsule keyword.
/// This intentionally uses the transient retain flag, rather than CardKeyword.Retain,
/// so a capsule presents one concise tooltip instead of three separate keywords.
/// </summary>
[HarmonyPatch]
internal static class CapsuleKeywordBehaviorPatch
{
    /// <summary>
    /// The game's flush decision happens immediately after Hook.BeforeFlush completes.
    /// Marking each capsule with one-turn retain here therefore preserves only those
    /// individual cards, without retaining the whole hand.
    /// </summary>
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeFlush))]
    [HarmonyPostfix]
    private static void RetainCapsulesBeforeHandFlush(ref Task __result, Player player)
    {
        __result = GiveCapsulesSingleTurnRetain(__result, player);
    }

    private static async Task GiveCapsulesSingleTurnRetain(Task originalTask, Player player)
    {
        await originalTask;

        CardPile? hand = CardPile.Get(PileType.Hand, player);
        if (hand == null)
        {
            return;
        }

        foreach (CardModel card in hand.Cards.Where(IsCapsule))
        {
            card.GiveSingleTurnRetain();
        }
    }

    /// <summary>
    /// A successful card play has finished only after OnPlayWrapper's task completes.
    /// Remove the persistent deck version at that point; a combat-only generated card
    /// has no DeckVersion and is already handled by the base game's normal pile cleanup.
    /// </summary>
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    [HarmonyPostfix]
    private static void RemovePlayedCapsuleFromDeck(ref Task __result, CardModel __instance)
    {
        __result = RemoveDeckVersionAfterSuccessfulPlay(__result, __instance);
    }

    private static async Task RemoveDeckVersionAfterSuccessfulPlay(Task originalTask, CardModel playedCard)
    {
        await originalTask;

        if (!IsCapsule(playedCard))
        {
            return;
        }

        CardModel? deckVersion = playedCard.DeckVersion;
        if (deckVersion?.Pile?.Type != PileType.Deck)
        {
            return;
        }

        await CardPileCmd.RemoveFromDeck(deckVersion, showPreview: false);
        MainFile.Logger.Info($"Capsule removed {deckVersion.Id.Entry} from the player's deck after play.", 1);
    }

    private static bool IsCapsule(CardModel card) =>
        card.Keywords.Contains(InstantPillKeywords.Capsule);
}
