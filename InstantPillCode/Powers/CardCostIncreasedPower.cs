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
/// Makes one normal energy-cost card per stack cost one additional energy. Stacks are consumed
/// only after an affected card actually spends energy, so the power applied by its source card
/// cannot consume itself.
/// </summary>
[CustomID("INSTANTPILL-CARD_COST_INCREASED")]
public sealed class CardCostIncreasedPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Temporary presentation until InstantPill receives dedicated power icons.
    public override string? CustomPackedIconPath => ModelDb.Power<ConfusedPower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<ConfusedPower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<ConfusedPower>().ResolvedBigIconPath;

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        if (card.Owner.Creature != Owner || card.EnergyCost.CostsX)
        {
            modifiedCost = originalCost;
            return false;
        }

        modifiedCost = originalCost + 1m;
        return true;
    }

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (amount > 0 && card.Owner.Creature == Owner && !card.EnergyCost.CostsX)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
