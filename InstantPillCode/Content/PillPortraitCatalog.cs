using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Content;

/// <summary>
/// Defines the packaged portrait resources for pills and resolves the run-local portrait used by
/// an identified effect pill. Effect cards deliberately borrow the mapped mystery pill's image.
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
    /// Resolves an effect card's portrait without changing run state. Canonical/library cards and
    /// effects outside this player's pool use the neutral fallback image.
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
        return mysteryPillId == null
            ? UsePackagedPortraitOrMissing(DefaultEffectPortraitPath)
            : GetMysteryPortraitPath(mysteryPillId);
    }

    private static string UsePackagedPortraitOrMissing(string portraitPath) =>
        ResourceLoader.Exists(portraitPath)
            ? portraitPath
            : CardModel.MissingPortraitPath;
}
