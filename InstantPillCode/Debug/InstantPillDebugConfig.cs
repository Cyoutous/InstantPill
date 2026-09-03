using System.Linq;
using BaseLib.Config;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Debug;

/// <summary>
/// Deliberately small, development-only controls for assigning and clearing the new keyword.
/// They change the current run's deck; use them outside combat, then enter a new combat.
/// </summary>
internal sealed class InstantPillDebugConfig : SimpleModConfig
{
    [ConfigButton("GRANT_CAPSULE_TO_FIRST_DECK_CARD_BUTTON")]
    private static void GrantCapsuleToFirstDeckCard()
    {
        Player? player = RunManager.Instance.DebugOnlyGetState()?.Players.FirstOrDefault();
        CardModel? card = player?.Deck.Cards.FirstOrDefault(candidate => !candidate.Keywords.Contains(InstantPillKeywords.Capsule));
        if (card == null)
        {
            MainFile.Logger.Warn("Test command did not find a deck card without the Capsule keyword. Start a run, or clear the keyword first.", 1);
            return;
        }

        card.AddKeyword(InstantPillKeywords.Capsule);
        MainFile.Logger.Info($"Test command granted Capsule to {card.Id} in the current run's deck. Start the next combat to test it.", 1);
    }

    [ConfigButton("CLEAR_CAPSULE_KEYWORDS_BUTTON")]
    private static void ClearCapsuleKeywords()
    {
        Player? player = RunManager.Instance.DebugOnlyGetState()?.Players.FirstOrDefault();
        if (player == null)
        {
            MainFile.Logger.Warn("Test command could not find an active run.", 1);
            return;
        }

        int cleared = 0;
        foreach (CardModel card in player.Deck.Cards.Where(candidate => candidate.Keywords.Contains(InstantPillKeywords.Capsule)))
        {
            card.RemoveKeyword(InstantPillKeywords.Capsule);
            cleared++;
        }

        MainFile.Logger.Info($"Test command removed Capsule from {cleared} deck card(s).", 1);
    }
}
