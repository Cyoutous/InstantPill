using System.Collections.Generic;
using BaseLib.Cards;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace InstantPill.InstantPillCode.Cards.Mystery;

/// <summary>
/// Shared no-effect card model for unidentified pills.
/// The later reveal system will decide their behaviour when they are played.
/// </summary>
public abstract class BaseMysteryPillCard : BasePillCard
{
    protected BaseMysteryPillCard()
        : base(0)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        InstantPillKeywords.Capsule,
        InstantPillKeywords.Mystery,
        BaseLibKeywords.Purge
    ];
}
