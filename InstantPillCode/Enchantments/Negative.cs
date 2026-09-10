using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Enchantments;

/// <summary>
/// A permanent card enchantment that draws one card whenever its attached card is normally drawn.
/// Negative deliberately leaves eligibility to <see cref="NegativeEnchantmentService"/>, because
/// the native generic enchant command excludes Status, Curse, and Quest cards.
/// </summary>
[CustomID("INSTANTPILL-NEGATIVE")]
public sealed class Negative : CustomEnchantmentModel
{
    /// <summary>
    /// Reuse Pale Blue Dot's original full-resolution Power icon.  Enchantments expect a PNG-backed
    /// texture path, so this deliberately does not use the Power's packed atlas (.tres) path.
    /// </summary>
    protected override string? CustomIconPath => ModelDb.Power<PaleBlueDotPower>().ResolvedBigIconPath;

    /// <summary>
    /// Shows the localized extraCardText entry directly on the enchanted card.
    /// </summary>
    public override bool HasExtraCardText => true;

    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        // This hook is broadcast to combat models.  Only react to the specific card carrying this enchantment.
        if (card != Card || Card.Pile?.Type != PileType.Hand)
        {
            return;
        }

        // Use the native draw path so hand capacity, reshuffling, animation, and other draw hooks remain intact.
        await CardPileCmd.Draw(choiceContext, 1m, Card.Owner);
    }
}
