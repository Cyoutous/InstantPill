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
/// False PHD's excluded Bad Trip result. It preserves only Bad Trip's harmful branch.
/// </summary>
[CustomID("INSTANTPILL-FPHD_BAD_TRIP")]
public sealed class FphdBadTrip : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/bad trip 1.wav";
    private const float SoundVolume = 1f;
    private const decimal HpLossPercent = 0.05m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        // Match Bad Trip's harmful branch exactly, including its blood-impact presentation.
        VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_bloody_impact");
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            Owner.Creature.MaxHp * HpLossPercent,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
            this,
            cardPlay);
    }
}
