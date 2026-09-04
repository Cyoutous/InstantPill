using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Cards;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// Shared structural model for identified effect pills.
/// Individual cards will add their gameplay behaviour when their effects are implemented.
/// </summary>
public abstract class BaseEffectPillCard : BasePillCard
{
    protected BaseEffectPillCard()
        : base(0)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        InstantPillKeywords.Capsule,
        BaseLibKeywords.Purge
    ];

    protected abstract int StrengthAmount { get; }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            StrengthAmount,
            Owner.Creature,
            this);
    }
}
