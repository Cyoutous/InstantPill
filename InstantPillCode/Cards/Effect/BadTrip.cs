using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-1 pill that is dangerous above half health, but restorative at or below half health.
/// Both values are calculated from the owner's current maximum HP at play time.
/// </summary>
[CustomID("INSTANTPILL-BAD_TRIP")]
public sealed class BadTrip : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/bad trip 1.wav";
    private const float SoundVolume = 1f;
    private const decimal LowHealthThreshold = 0.5m;
    private const decimal HpLossPercent = 0.05m;
    private const decimal HealPercent = 0.15m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    // These are the original card-frame glow hooks.  Their conditions mirror the
    // branch used on play, so the hand preview always communicates the result.
    protected override bool ShouldGlowGoldInternal => IsAtOrBelowHalfHealth;

    protected override bool ShouldGlowRedInternal => CombatState != null && !IsAtOrBelowHalfHealth;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        decimal maxHp = Owner.Creature.MaxHp;
        if (IsAtOrBelowHalfHealth)
        {
            await CreatureCmd.Heal(Owner.Creature, maxHp * HealPercent);
            return;
        }

        // Use the same blood-impact presentation as Breakthrough and Something's Wrong
        // before applying the unblockable HP loss.
        VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_bloody_impact");
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            maxHp * HpLossPercent,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this,
            cardPlay);
    }

    private bool IsAtOrBelowHalfHealth =>
        CombatState != null && Owner.Creature.CurrentHp <= Owner.Creature.MaxHp * LowHealthThreshold;
}
