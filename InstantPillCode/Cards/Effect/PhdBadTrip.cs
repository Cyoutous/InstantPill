using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// PHD-only replacement for Bad Trip. Both health branches heal; the low-health branch retains
/// Bad Trip's larger 15% recovery.
/// </summary>
[CustomID("INSTANTPILL-PHD_BAD_TRIP")]
public sealed class PhdBadTrip : BaseEffectPillCard
{
    private const decimal LowHealthThreshold = 0.5m;
    private const decimal NormalHealPercent = 0.05m;
    private const decimal LowHealthHealPercent = 0.15m;

    public PhdBadTrip()
        : base(showInCardLibrary: false)
    {
    }

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;

    protected override bool ShouldGlowGoldInternal => CombatState != null;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal healPercent = IsAtOrBelowHalfHealth ? LowHealthHealPercent : NormalHealPercent;
        await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp * healPercent);
    }

    private bool IsAtOrBelowHalfHealth =>
        CombatState != null && Owner.Creature.CurrentHp <= Owner.Creature.MaxHp * LowHealthThreshold;
}
