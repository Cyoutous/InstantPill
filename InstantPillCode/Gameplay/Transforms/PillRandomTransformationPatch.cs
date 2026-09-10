using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using InstantPill.InstantPillCode.Cards;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Transforms;

/// <summary>
/// Redirects random transformations of a capsule to one of the current run's capsule-pool
/// identities. Explicit replacements are not constructed by CardFactory and remain untouched:
/// mystery revelation, Question Marks, and effects such as Infested retain their specified targets.
/// </summary>
[HarmonyPatch]
internal static class PillRandomTransformationPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(
            typeof(CardFactory),
            nameof(CardFactory.CreateRandomCardForTransform),
            [typeof(CardModel), typeof(bool), typeof(Rng)]);
        yield return AccessTools.Method(
            typeof(CardFactory),
            nameof(CardFactory.CreateRandomCardForTransform),
            [typeof(CardModel), typeof(System.Collections.Generic.IEnumerable<CardModel>), typeof(bool), typeof(Rng)]);
    }

    [HarmonyPrefix]
    private static bool ReplaceRandomTransformationTarget(
        CardModel original,
        Rng rng,
        ref CardModel __result)
    {
        if (original is not BasePillCard)
        {
            return true;
        }

        string[] candidateIds = PillPoolService.EnsureInitialized(original.Owner)
            .CapsuleSlots
            .Select(slot => slot.CurrentCardId)
            .Distinct(StringComparer.Ordinal)
            .Where(cardId => !string.Equals(cardId, original.Id.Entry, StringComparison.Ordinal))
            .ToArray();
        if (candidateIds.Length == 0)
        {
            throw new InvalidOperationException(
                "InstantPill cannot randomly transform a capsule because this run has no alternate capsule-pool identity.");
        }

        string? selectedSourceId = rng.NextItem(candidateIds);
        if (selectedSourceId == null)
        {
            throw new InvalidOperationException("InstantPill could not select a random capsule transformation target.");
        }

        string selectedId = PillPoolService.ResolveCapsuleCardIdForPlayer(original.Owner, selectedSourceId);
        CardModel canonicalCard = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), selectedId));
        ICardScope scope = original.CardScope
            ?? throw new InvalidOperationException("InstantPill cannot transform a capsule outside a card scope.");
        __result = scope.CreateCard(canonicalCard, original.Owner);
        MainFile.Logger.Info(
            $"Redirected random capsule transformation {original.Id.Entry} -> {selectedSourceId} -> {selectedId} for player {original.Owner.NetId}.",
            1);
        return false;
    }
}
