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

/// <summary>A grade-2 pill that expands orb capacity, then channels Lightning and Frost.</summary>
[CustomID("INSTANTPILL-PRETTY_FLY")]
public sealed class PrettyFly : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/pretty fly 5.wav";
    private const float SoundVolume = 1f;
    private const int SlotAmount = 1;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-LUCK_DOWN";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromOrb<LightningOrb>(),
        HoverTipFactory.FromOrb<FrostOrb>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await OrbCmd.Channel<LightningOrb>(choiceContext, Owner);
        await OrbCmd.AddSlots(Owner, SlotAmount);
        await OrbCmd.Channel<FrostOrb>(choiceContext, Owner);
    }
}
