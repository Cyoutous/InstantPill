using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Gameplay.Effects;
using InstantPill.InstantPillCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-0 pill which plays the owner's standard PowerUp animation and awards maximum HP only
/// on its third run-wide use.
/// </summary>
[CustomID("INSTANTPILL-PUBERTY")]
public sealed class Puberty : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/puberty 1.wav";
    private const float SoundVolume = 1f;
    private const int UsesUntilMaxHpGain = 3;
    private const decimal MaxHpGain = 6m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int playCount = PubertyPlayCounter.Increment(Owner);

        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        // This is the exact character animation path used by Serpent Form, Demon Form, and Echo Form.
        await CreatureCmd.TriggerAnim(
            Owner.Creature,
            "PowerUp",
            Owner.Character.PowerUpAnimDelay);

        await PowerCmd.Apply<PubertyPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature,
            this);

        if (playCount == UsesUntilMaxHpGain)
        {
            // GainMaxHp follows the original-game convention: increase max HP, then heal that amount.
            await CreatureCmd.GainMaxHp(Owner.Creature, MaxHpGain);
        }
    }
}
