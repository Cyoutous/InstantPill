using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-2 pill that channels one original Plasma orb.</summary>
[CustomID("INSTANTPILL-48_HOUR_ENERGY")]
public sealed class FortyEightHourEnergy : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/48 hr energy 6.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-SPEED_DOWN";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromOrb<PlasmaOrb>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await OrbCmd.Channel<PlasmaOrb>(choiceContext, Owner);
    }
}
