using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using Godot;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-2 pill that applies the original Slippery defensive power.</summary>
[CustomID("INSTANTPILL-X_LAX")]
public sealed class XLax : BaseEffectPillCard
{
    private static readonly Color BlackSplashTint = new(0f, 0f, 0f, 1f);

    private const string SoundPath = "res://audio/x lax 2.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-SOMETHINGS_WRONG";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SlipperyPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        NCombatRoom.Instance?.PlaySplashVfx(Owner.Creature, BlackSplashTint);
        await PowerCmd.Apply<SlipperyPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }
}
