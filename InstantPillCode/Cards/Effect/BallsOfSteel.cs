using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-2 pill that gains Block and preserves it through the next player turn start.</summary>
[CustomID("INSTANTPILL-BALLS_OF_STEEL")]
public sealed class BallsOfSteel : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/balls of steel 4.wav";
    private const float SoundVolume = 1f;
    private const decimal BlurAmount = 1m;

    protected override int StrengthAmount => 0;

    public override bool GainsBlock => true;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(16m, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<BlurPower>(
            choiceContext, Owner.Creature, BlurAmount, Owner.Creature, this);
    }
}
