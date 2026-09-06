using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-3 pill that restores its owner's HP to their maximum.</summary>
[CustomID("INSTANTPILL-FULL_HEALTH")]
public sealed class FullHealth : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/full health 1.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        // Heal caps at max HP, so the owner's max HP is sufficient to restore all missing HP.
        await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp);
    }
}
