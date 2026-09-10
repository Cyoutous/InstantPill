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
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-2 pill that uses the vanilla Shrink power and makes the next two cards free.
/// </summary>
[CustomID("INSTANTPILL-ONE_MAKES_YOU_SMALL")]
public sealed class OneMakesYouSmall : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/small_05.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<ShrinkPower>(),
        ModelDb.Power<FreeNextCardPower>().GetDumbHoverTip(amountOverride: 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await PowerCmd.Apply<ShrinkPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
        await PowerCmd.Apply<FreeNextCardPower>(
            choiceContext,
            Owner.Creature,
            2m,
            Owner.Creature,
            this);
    }
}
