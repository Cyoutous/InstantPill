using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A harmless visual fake-out: the owner cannot be reduced below 1 HP, but their
/// combat avatar remains in its death pose until another animation replaces it.
/// </summary>
[CustomID("INSTANTPILL-SOMETHINGS_WRONG")]
public sealed class SomethingsWrong : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/wrong 1.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Match Breakthrough's HP-loss presentation: its damage command is what
        // produces the red number, while this VFX provides the bloody impact.
        VfxCmd.PlayOnCreatureCenter(Owner.Creature, "vfx/vfx_bloody_impact");
        if (Owner.Creature.CurrentHp > 1)
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner.Creature,
                1m,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this,
                cardPlay);
        }

        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        // Do not call CreatureCmd.Kill or NCreature.StartDeathAnim: both enter the
        // actual death pipeline.  The animation trigger only changes the visual state.
        NCombatRoom.Instance?.GetCreatureNode(Owner.Creature)?.SetAnimationTrigger("Dead");
    }
}
