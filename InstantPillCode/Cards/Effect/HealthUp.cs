using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-3 pill that gains 8 max HP and immediately heals the same amount.</summary>
[CustomID("INSTANTPILL-HEALTH_UP")]
public sealed class HealthUp : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/health up 1.wav";
    private const float SoundVolume = 1f;
    private const decimal MaxHpGain = 8m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        // The native command raises max HP and heals that same amount, capped by the new maximum.
        await CreatureCmd.GainMaxHp(Owner.Creature, MaxHpGain);
    }
}
