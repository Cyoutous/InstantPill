using System.Collections.Generic;
using BaseLib.Cards;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

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
}
