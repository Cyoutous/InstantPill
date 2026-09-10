using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// PHD-only replacement for I'm Excited. It retains the draw and energy effects while removing
/// the original pill's enemy temporary-Strength drawback.
/// </summary>
[CustomID("INSTANTPILL-PHD_IM_EXCITED")]
public sealed class PhdImExcited : BaseEffectPillCard
{
    private const decimal CardsToDraw = 5m;
    private const int EnergyToGain = 2;

    public PhdImExcited()
        : base(showInCardLibrary: false)
    {
    }

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(EnergyToGain)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        EnergyHoverTip
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, CardsToDraw, Owner);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }
}
