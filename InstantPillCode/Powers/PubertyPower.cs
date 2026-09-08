using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Powers;

/// <summary>
/// A temporary counter: each of the owner's next Attack cards this turn grants five Block and
/// consumes one stack. Any unused stacks are removed at the end of that owner's side turn.
/// </summary>
[CustomID("INSTANTPILL-PUBERTY")]
public sealed class PubertyPower : CustomPowerModel
{
    private const decimal BlockPerAttack = 5m;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Intentionally reuse the original Rage icon until this power receives dedicated art.
    public override string? CustomPackedIconPath => ModelDb.Power<RagePower>().PackedIconPath;

    public override string? CustomBigIconPath => ModelDb.Power<RagePower>().ResolvedBigIconPath;

    public override string? CustomBigBetaIconPath => ModelDb.Power<RagePower>().ResolvedBigIconPath;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.Static(StaticHoverTip.Block)
    ];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = cardPlay.Card;
        if (card.Owner.Creature != Owner || card.Type != CardType.Attack)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner, BlockPerAttack, ValueProp.Unpowered, cardPlay, fast: true);
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
