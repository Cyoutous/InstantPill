using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs.History;

namespace InstantPill.InstantPillCode.Enchantments;

/// <summary>
/// Applies Negative while intentionally supporting every real CardType, including Status, Curse, and Quest.
/// The native CardCmd.Enchant eligibility check rejects those types, so this service mirrors its successful
/// application path without changing the native eligibility rules globally.
/// </summary>
public static class NegativeEnchantmentService
{
    /// <summary>
    /// Applies Negative if the card has no existing enchantment. Returns null when it is already enchanted.
    /// </summary>
    public static Negative? TryApply(CardModel card)
    {
        ArgumentNullException.ThrowIfNull(card);

        if (card.Enchantment is not null)
        {
            return null;
        }

        Negative enchantment = (Negative)ModelDb.Enchantment<Negative>().ToMutable();
        card.EnchantInternal(enchantment, 1m);
        enchantment.ModifyCard();
        card.FinalizeUpgradeInternal();

        RecordDeckHistory(card, enchantment);
        return enchantment;
    }

    private static void RecordDeckHistory(CardModel card, Negative enchantment)
    {
        if (card.Pile?.Type != PileType.Deck)
        {
            return;
        }

        var historyEntry = card.Owner.RunState.CurrentMapPointHistoryEntry;
        historyEntry?.GetEntry(card.Owner.NetId).CardsEnchanted.Add(
            new CardEnchantmentHistoryEntry(card, enchantment.Id));
    }
}
