using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// PHD-only replacement for I'm Drowsy. It leaves the player and pets untouched while applying
/// the original Dark Shackles temporary Strength loss to all enemies.
/// </summary>
[CustomID("INSTANTPILL-PHD_IM_DROWSY")]
public sealed class PhdImDrowsy : BaseEffectPillCard
{
    private const decimal StrengthLoss = 8m;

    public PhdImDrowsy()
        : base(showInCardLibrary: false)
    {
    }

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        List<Creature> targets =
        [
            .. CombatState.Enemies
        ];
        await PowerCmd.Apply<DarkShacklesPower>(
            choiceContext,
            targets,
            StrengthLoss,
            Owner.Creature,
            this);
    }
}
