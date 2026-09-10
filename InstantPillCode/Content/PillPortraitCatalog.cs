using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using InstantPill.InstantPillCode.Cards;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Entities.Players;
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

    // This is used for effects which have no run-local mystery mapping, including the card
    // library's canonical thumbnails and other ownerless previews.
    public static string DefaultMysteryPortraitPath => CardModel.MissingPortraitPath;

    public static string DefaultEffectPortraitPath => EffectPortraitDirectory + "/effect_pill_unknown.png";

    public static IReadOnlyList<string> AllMysteryPortraitPaths { get; } =
    [
        .. PillPoolCatalog.MysteryPillIds.Select(GetMysteryPortraitPath)
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
        if (PillCardLibraryPortraitContext.TryGetPortrait(effectPill.Id.Entry, out string libraryPortraitPath))
        {
            return libraryPortraitPath;
        }

        if (effectPill.IsCanonical)
        {
            return DefaultEffectPortraitPath;
        }

        // NInspectCardScreen creates an ownerless mutable clone before rendering a card. It is
        // not a combat card and must never attempt to read a player's run-local capsule pool.
        Player? owner = effectPill.Owner;
        if (owner == null)
        {
            return DefaultEffectPortraitPath;
        }

        string? mysteryPillId = PillPoolService.TryGetMysteryPillIdForAssignedEffect(
            owner,
            effectPill.Id.Entry);
        if (mysteryPillId != null)
        {
            return GetMysteryPortraitPath(mysteryPillId);
        }

        // A PHD result which was not selected into this run's candidate pool inherits the
        // mystery portrait of its first selected PHD source. The relationship is derived from
        // static card metadata plus slot order and remains independent of save-state mutation.
        string? phdInheritedMysteryPillId = PillPoolService.TryGetPhdInheritedMysteryPillId(
            owner,
            effectPill.Id.Entry);
        if (phdInheritedMysteryPillId != null)
        {
            return GetMysteryPortraitPath(phdInheritedMysteryPillId);
        }

        string? questionMarksMysteryPillId = PillPoolService.TryGetMysteryPillIdForAssignedEffect(
            owner,
            QuestionMarks.CardId);
        return questionMarksMysteryPillId == null
            ? DefaultEffectPortraitPath
            : GetMysteryPortraitPath(questionMarksMysteryPillId);
    }

    private static string UsePackagedPortraitOrMissing(string portraitPath) =>
        ResourceLoader.Exists(portraitPath)
            ? portraitPath
            : CardModel.MissingPortraitPath;
}
