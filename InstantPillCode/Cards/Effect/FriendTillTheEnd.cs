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

/// <summary>A grade-1 pill that channels the original Glass orb.</summary>
[CustomID("INSTANTPILL-FRIEND_TILL_THE_END")]
public sealed class FriendTillTheEnd : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/friends 1.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-HEALTH_DOWN";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromOrb<GlassOrb>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await OrbCmd.Channel<GlassOrb>(choiceContext, Owner);
    }
}
