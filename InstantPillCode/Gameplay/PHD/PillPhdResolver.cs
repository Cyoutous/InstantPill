using System;
using System.Linq;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Gameplay.PHD;

/// <summary>
/// Resolves the effect-card identity a particular player should actually receive or play.
/// It deliberately never changes the run-local mystery mapping: PHD is a per-player
/// materialization rule layered on top of that immutable mapping.
/// </summary>
public static class PillPhdResolver
{
    public static bool HasPhd(Player player) => player.Relics.Any(relic =>
        string.Equals(relic.Id.Entry, PhdRelic.RelicId, StringComparison.Ordinal));

    public static bool HasFalsePhd(Player player) => player.Relics.Any(relic =>
        string.Equals(relic.Id.Entry, FalsePhdRelic.RelicId, StringComparison.Ordinal));

    /// <summary>
    /// Resolves at most one pharmacy-relic replacement. PHD and False PHD cancel each other,
    /// leaving the original effect intact. Replacement chains are intentionally not followed.
    /// </summary>
    public static string ResolveEffectCardId(Player player, string originalEffectCardId)
    {
        bool hasPhd = HasPhd(player);
        bool hasFalsePhd = HasFalsePhd(player);
        if (hasPhd == hasFalsePhd)
        {
            return originalEffectCardId;
        }

        BaseEffectPillCard effectPill = GetEffectPill(originalEffectCardId);
        string? replacementId = hasPhd
            ? effectPill.PhdReplacementCardId
            : effectPill.FalsePhdReplacementCardId;
        return string.IsNullOrWhiteSpace(replacementId)
            ? originalEffectCardId
            : replacementId;
    }

    /// <summary>
    /// Validates every registered PHD and False PHD replacement declaration without consuming RNG.
    /// </summary>
    public static void ValidateReplacementMetadata(System.Collections.Generic.IEnumerable<string> effectCardIds)
    {
        string[] catalogIds = effectCardIds.ToArray();
        foreach (string effectCardId in catalogIds)
        {
            BaseEffectPillCard effectPill = GetEffectPill(effectCardId);
            ValidateReplacementMetadata(
                catalogIds,
                effectCardId,
                effectPill.PhdReplacementCardId,
                "PHD");
            ValidateReplacementMetadata(
                catalogIds,
                effectCardId,
                effectPill.FalsePhdReplacementCardId,
                "False PHD");
        }
    }

    private static void ValidateReplacementMetadata(
        string[] catalogIds,
        string effectCardId,
        string? replacementId,
        string relicName)
    {
        if (string.IsNullOrWhiteSpace(replacementId))
        {
            return;
        }

        if (string.Equals(effectCardId, replacementId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"InstantPill {relicName} replacement for {effectCardId} cannot target itself.");
        }

        if (!catalogIds.Contains(replacementId, StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"InstantPill {relicName} replacement {effectCardId} -> {replacementId} targets a card outside the effect-pill catalog.");
        }

        _ = GetEffectPill(replacementId);
    }

    private static BaseEffectPillCard GetEffectPill(string effectCardId)
    {
        CardModel canonicalCard = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), effectCardId));
        return canonicalCard as BaseEffectPillCard
            ?? throw new InvalidOperationException(
                $"InstantPill PHD target {effectCardId} is not a {nameof(BaseEffectPillCard)}.");
    }
}
