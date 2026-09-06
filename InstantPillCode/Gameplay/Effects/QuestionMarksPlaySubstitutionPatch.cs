using System;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Effects;

/// <summary>
/// Replaces the played Question Marks combat copy with one effect from the complete registered
/// effect catalogue. Unlike mystery-pill revelation, this neither reads nor mutates either pool.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
internal static class QuestionMarksPlaySubstitutionPatch
{
    [HarmonyPrefix]
    private static bool SubstituteQuestionMarks(
        CardModel __instance,
        ref Task __result,
        PlayerChoiceContext choiceContext,
        Creature? target,
        bool isAutoPlay,
        ResourceInfo resources,
        bool skipCardPileVisuals)
    {
        if (__instance is not QuestionMarks)
        {
            return true;
        }

        string[] candidates = PillPoolCatalog.EffectPillIds
            .Where(id => !string.Equals(id, __instance.Id.Entry, StringComparison.Ordinal))
            .Where(PillPoolService.IsEligibleEffectPill)
            .ToArray();
        if (candidates.Length == 0)
        {
            MainFile.Logger.Warn("InstantPill could not find an effect for Question Marks.");
            return true;
        }

        string selectedEffectId = candidates[
            __instance.Owner.RunState.Rng.CombatCardGeneration.NextInt(candidates.Length)];
        __result = TransformAndPlayEffect(
            __instance,
            selectedEffectId,
            choiceContext,
            target,
            isAutoPlay,
            resources,
            skipCardPileVisuals);
        return false;
    }

    private static async Task TransformAndPlayEffect(
        CardModel questionMarks,
        string selectedEffectId,
        PlayerChoiceContext choiceContext,
        Creature? target,
        bool isAutoPlay,
        ResourceInfo resources,
        bool skipCardPileVisuals)
    {
        if (LocalContext.IsMine(questionMarks))
        {
            // This is the only card-authored sound heard for the proxy play.
            QuestionMarks.PlayRandomSound();
        }

        CardModel canonicalEffect = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), selectedEffectId));
        ICardScope scope = questionMarks.CardScope
            ?? throw new InvalidOperationException("InstantPill cannot trigger Question Marks outside a card scope.");
        CardModel replacement = scope.CreateCard(canonicalEffect, questionMarks.Owner);
        replacement.DeckVersion = questionMarks.DeckVersion;

        CardPileAddResult? transformResult = await CardCmd.Transform(
            questionMarks,
            replacement,
            CardPreviewStyle.None);
        if (transformResult is not { success: true })
        {
            throw new InvalidOperationException("InstantPill failed to substitute Question Marks with its selected effect.");
        }

        // The replacement executes normally, including gameplay VFX. Its custom card audio is
        // muted for this async flow so Question Marks remains the sole authored voice cue.
        using (PillAudio.SuppressCustomCardSounds())
        {
            await transformResult.Value.cardAdded.OnPlayWrapper(
                choiceContext,
                target,
                isAutoPlay,
                resources,
                skipCardPileVisuals);
        }
    }
}
