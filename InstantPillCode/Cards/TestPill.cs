using System.Collections.Generic;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace InstantPill.InstantPillCode.Cards;

/// <summary>
/// Minimal verification card for the pill framework.
/// It has no play effect: Capsule handles opening-hand entry, end-of-turn retention,
/// and removal from the player's deck after play.
/// </summary>
public sealed class TestPill : BasePillCard
{
    public TestPill()
        : base(0)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        InstantPillKeywords.Capsule
    ];
}
