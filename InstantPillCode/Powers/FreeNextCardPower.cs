using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Powers;

/// <summary>
/// Makes one card per stack free. Its late cost modifier deliberately wins over normal global
/// cost modifiers, matching the vanilla Free Attack/Skill/Power effects.
/// </summary>
[CustomID("INSTANTPILL-FREE_NEXT_CARD")]
public sealed class FreeNextCardPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Temporary presentation until InstantPill receives a dedicated power icon.
    public override string? CustomPackedIconPath => ModelDb.Power<FreePowerPower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<FreePowerPower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<FreePowerPower>().ResolvedBigIconPath;

    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner.Creature != Owner || !IsInPlayablePile(card))
        {
            return false;
        }

        modifiedCost = 0m;
        return true;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;
        if (card.Owner.Creature == Owner && IsInPlayablePile(card))
        {
            // This also consumes a stack for X-cost cards, matching vanilla free-card powers.
            await PowerCmd.Decrement(this);
        }
    }

    private static bool IsInPlayablePile(CardModel card) => card.Pile?.Type is PileType.Hand or PileType.Play;
}
