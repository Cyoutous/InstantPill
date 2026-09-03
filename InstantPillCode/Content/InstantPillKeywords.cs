using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace InstantPill.InstantPillCode.Content;

/// <summary>
/// Keywords owned by InstantPill. BaseLib assigns the enum value during ModelDb initialization.
/// </summary>
public static class InstantPillKeywords
{
    [CustomEnum("CAPSULE")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Capsule;

    [CustomEnum("MYSTERY")]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Mystery;
}
