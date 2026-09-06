using System;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-2 pill that lets its owner inspect the real order of their draw pile for this combat.
/// </summary>
[CustomID("INSTANTPILL-I_CAN_SEE_FOREVER")]
public sealed class ICanSeeForever : BaseEffectPillCard
{
    private static readonly string[] SoundPaths =
    [
        "res://audio/see 4ever 1.wav",
        "res://audio/see 4ever 2.wav"
    ];

    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPaths[Random.Shared.Next(SoundPaths.Length)], SoundVolume);
        }

        await PowerCmd.Apply<ICanSeeForeverPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);
    }
}
