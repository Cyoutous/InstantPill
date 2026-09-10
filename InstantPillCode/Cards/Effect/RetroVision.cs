using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Gameplay.Vfx;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-0 pill that briefly applies InstantPill's local-only full-screen pixelation effect.
/// </summary>
[CustomID("INSTANTPILL-RETRO_VISION")]
public sealed class RetroVision : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/retro_05.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade0;

    public override string? PhdReplacementCardId => "INSTANTPILL-I_CAN_SEE_FOREVER";

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        PillScreenEffects.PlayPixelation(this);
        return Task.CompletedTask;
    }
}
