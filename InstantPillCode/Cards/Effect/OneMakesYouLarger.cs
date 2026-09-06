using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-2 pill that enlarges its player for the current turn and taxes the next played card.
/// </summary>
[CustomID("INSTANTPILL-ONE_MAKES_YOU_LARGER")]
public sealed class OneMakesYouLarger : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/larger_03.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        new HoverTip(
            ModelDb.Power<EnlargedPower>(),
            ModelDb.Power<EnlargedPower>().Description.GetFormattedText(),
            isSmart: false),
        ModelDb.Power<CardCostIncreasedPower>().GetDumbHoverTip(amountOverride: 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await PowerCmd.Apply<EnlargedPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
        await PowerCmd.Apply<CardCostIncreasedPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }
}
