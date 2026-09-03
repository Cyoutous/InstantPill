using System.Collections.Generic;
using BaseLib.Cards;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace InstantPill.InstantPillCode.Cards;

/// <summary>
/// Minimal verification card for the pill framework.
/// It has no play effect: Capsule handles opening-hand entry and Purge removes its deck version after play.
/// </summary>
public sealed class TestPill : BasePillCard
{
    public TestPill()
        : base(0)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        InstantPillKeywords.Capsule,
        BaseLibKeywords.Purge
    ];
}
