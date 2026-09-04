using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-0 pill that plays its own relaxation cue before Neow's vanilla welcome voice event.
/// </summary>
[CustomID("INSTANTPILL-RE_LAX")]
public sealed class ReLax : BaseEffectPillCard
{
    private const string RelaxSoundPath = "res://audio/relax 1.wav";
    private const float RelaxSoundVolume = 1f;
    private const int RelaxSoundDurationMilliseconds = 1769;
    private const string NeowWelcomeSfx = "event:/sfx/npcs/neow/neow_welcome";

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Audio playback is presentation-only. Each player hears this pill on their own client.
        if (!LocalContext.IsMine(this))
        {
            return;
        }

        PillAudio.PlayOneShot(RelaxSoundPath, RelaxSoundVolume);
        await Task.Delay(RelaxSoundDurationMilliseconds);
        SfxCmd.Play(NeowWelcomeSfx);
    }
}
