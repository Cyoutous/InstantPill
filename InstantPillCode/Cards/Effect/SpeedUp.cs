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

/// <summary>
/// A grade-3 pill that permanently adds one original Master of Strategy to the player's deck.
/// </summary>
[CustomID("INSTANTPILL-SPEED_UP")]
public sealed class SpeedUp : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/speed up 2.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<MasterOfStrategy>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        MasterOfStrategy masterOfStrategy = Owner.RunState.CreateCard<MasterOfStrategy>(Owner);
        CardPileAddResult result = await CardPileCmd.Add(masterOfStrategy, PileType.Deck);
        CardCmd.PreviewCardPileAdd([result], 2f);
    }
}
