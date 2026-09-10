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

    /// <summary>
    /// Resolves at most one PHD replacement. Replacement chains are intentionally not followed.
    /// </summary>
    public static string ResolveEffectCardId(Player player, string originalEffectCardId)
    {
        if (!HasPhd(player))
        {
            return originalEffectCardId;
        }

        BaseEffectPillCard effectPill = GetEffectPill(originalEffectCardId);
        return string.IsNullOrWhiteSpace(effectPill.PhdReplacementCardId)
            ? originalEffectCardId
            : effectPill.PhdReplacementCardId;
    }

    /// <summary>Validates every registered PHD replacement declaration without consuming RNG.</summary>
    public static void ValidateReplacementMetadata(System.Collections.Generic.IEnumerable<string> effectCardIds)
    {
        string[] catalogIds = effectCardIds.ToArray();
        foreach (string effectCardId in catalogIds)
        {
            BaseEffectPillCard effectPill = GetEffectPill(effectCardId);
            string? replacementId = effectPill.PhdReplacementCardId;
            if (string.IsNullOrWhiteSpace(replacementId))
            {
                continue;
            }

            if (string.Equals(effectCardId, replacementId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"InstantPill PHD replacement for {effectCardId} cannot target itself.");
            }

            if (!catalogIds.Contains(replacementId, StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    $"InstantPill PHD replacement {effectCardId} -> {replacementId} targets a card outside the effect-pill catalog.");
            }

            _ = GetEffectPill(replacementId);
        }
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
