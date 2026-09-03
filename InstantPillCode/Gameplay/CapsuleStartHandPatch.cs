using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Gameplay;

/// <summary>
/// Resolves the Capsule keyword after normal BeforeHandDraw listeners but before
/// CombatManager performs the opening hand draw (and its native Innate processing).
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeHandDraw))]
internal static class CapsuleStartHandPatch
{
    [HarmonyPostfix]
    private static void AddCapsulesBeforeOpeningDraw(
        ref Task __result,
        ICombatState combatState,
        Player player,
        PlayerChoiceContext playerChoiceContext)
    {
        __result = MoveCapsulesToHand(__result, player);
    }

    private static async Task MoveCapsulesToHand(Task originalTask, Player player)
    {
        await originalTask;

        if (player.PlayerCombatState?.TurnNumber != 1)
        {
            return;
        }

        CardPile drawPile = player.PlayerCombatState.DrawPile;
        CardPile? hand = CardPile.Get(PileType.Hand, player);
        if (hand == null)
        {
            MainFile.Logger.Warn("Capsule start-hand effect could not find the player's hand pile.", 1);
            return;
        }

        // Snapshot the current shuffled draw-pile order. CardPileCmd.Add moves these exact
        // combat instances; it does not invoke CardPileCmd.Draw or draw-related card hooks.
        CardModel[] capsules = drawPile.Cards
            .Where(card => card.Keywords.Contains(InstantPillKeywords.Capsule))
            .ToArray();

        foreach (CardModel capsule in capsules)
        {
            if (hand.Cards.Count >= CardPile.MaxCardsInHand)
            {
                break;
            }

            await CardPileCmd.Add(capsule, hand, CardPilePosition.Bottom);
        }

        if (capsules.Length > 0)
        {
            MainFile.Logger.Info($"Capsule start-hand effect moved up to {capsules.Length} card(s) before the opening draw.", 1);
        }
    }
}
