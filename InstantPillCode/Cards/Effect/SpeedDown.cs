using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-3 pill that permanently adds one original Clumsy curse to the player's deck.</summary>
[CustomID("INSTANTPILL-SPEED_DOWN")]
public sealed class SpeedDown : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/speed down 2.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    public override string? PhdReplacementCardId => "INSTANTPILL-SPEED_UP";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Clumsy>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        Clumsy clumsy = Owner.RunState.CreateCard<Clumsy>(Owner);
        CardPileAddResult result = await CardPileCmd.Add(clumsy, PileType.Deck);
        CardCmd.PreviewCardPileAdd([result], 2f);
    }
}
