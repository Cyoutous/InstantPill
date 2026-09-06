using System;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-1 pill whose actual effect is selected and played by
/// <see cref="Gameplay.Effects.QuestionMarksPlaySubstitutionPatch"/>.
/// </summary>
[CustomID("INSTANTPILL-QUESTION_MARKS")]
public sealed class QuestionMarks : BaseEffectPillCard
{
    private static readonly string[] SoundPaths =
    [
        "res://audio/___-3.wav",
        "res://audio/___.wav",
        "res://audio/___-2.wav"
    ];

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    // This fallback is intentionally inert. The Harmony play-wrapper patch always supplies the
    // selected effect card before a normal play begins.
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

    internal static void PlayRandomSound()
    {
        PillAudio.PlayOneShot(SoundPaths[Random.Shared.Next(SoundPaths.Length)]);
    }
}
