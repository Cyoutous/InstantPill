using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Content;

/// <summary>
/// Defines the packaged portrait resources for pills and resolves the run-local portrait used by
/// an identified effect pill. Effect cards deliberately borrow the mapped mystery pill's image.
/// If Question Marks is present in this run's effect pool, off-pool proxy effects borrow its
/// mystery image as well.
/// </summary>
public static class PillPortraitCatalog
{
    private const string MysteryPortraitDirectory = "res://InstantPill/images/cards/mystery";
    private const string EffectPortraitDirectory = "res://InstantPill/images/cards/effect";

    public const string DefaultMysteryPortraitPath = MysteryPortraitDirectory + "/mystery_pill_unknown.png";

    public const string DefaultEffectPortraitPath = EffectPortraitDirectory + "/effect_pill_unknown.png";

    public static IReadOnlyList<string> AllMysteryPortraitPaths { get; } =
    [
        .. PillPoolCatalog.MysteryPillIds.Select(GetMysteryPortraitPath),
        DefaultMysteryPortraitPath
    ];

    public static IReadOnlyList<string> AllEffectPortraitPaths { get; } =
    [
        .. AllMysteryPortraitPaths,
        DefaultEffectPortraitPath
    ];

    /// <summary>
    /// Returns the fixed packaged image for one mystery identity. The ID suffix is intentionally
    /// the only portion encoded into the resource name, keeping effect IDs free to become
    /// descriptive names later.
    /// </summary>
    public static string GetMysteryPortraitPath(string mysteryPillId)
    {
        if (!PillPoolCatalog.MysteryPillIds.Contains(mysteryPillId, StringComparer.Ordinal))
        {
            return DefaultMysteryPortraitPath;
        }

        string suffix = mysteryPillId[^3..].ToLowerInvariant();
        return UsePackagedPortraitOrMissing($"{MysteryPortraitDirectory}/mystery_pill_{suffix}.png");
    }

    /// <summary>
    /// Resolves an effect card's portrait without changing run state. Canonical/library cards use
    /// the neutral fallback image. Effects outside this player's pool borrow Question Marks'
    /// mapped mystery image whenever it is one of this run's candidate effects; this keeps a
    /// Question Marks proxy play visually coherent without mutating either pool.
    /// </summary>
    public static string GetEffectPortraitPath(CardModel effectPill)
    {
        if (effectPill.IsCanonical)
        {
            return UsePackagedPortraitOrMissing(DefaultEffectPortraitPath);
        }

        string? mysteryPillId = PillPoolService.TryGetMysteryPillIdForAssignedEffect(
            effectPill.Owner,
            effectPill.Id.Entry);
        if (mysteryPillId != null)
        {
            return GetMysteryPortraitPath(mysteryPillId);
        }

        string? questionMarksMysteryPillId = PillPoolService.TryGetMysteryPillIdForAssignedEffect(
            effectPill.Owner,
            QuestionMarks.CardId);
        return questionMarksMysteryPillId == null
            ? UsePackagedPortraitOrMissing(DefaultEffectPortraitPath)
            : GetMysteryPortraitPath(questionMarksMysteryPillId);
    }

    private static string UsePackagedPortraitOrMissing(string portraitPath) =>
        ResourceLoader.Exists(portraitPath)
            ? portraitPath
            : CardModel.MissingPortraitPath;
}
