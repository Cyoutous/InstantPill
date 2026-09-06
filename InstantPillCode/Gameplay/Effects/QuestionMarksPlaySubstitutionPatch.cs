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
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
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

        if (!TrySelectRandomEffect(__instance.Owner, __instance.Id.Entry, out string selectedEffectId))
        {
            MainFile.Logger.Warn("InstantPill could not find an effect for Question Marks.");
            return true;
        }

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

    /// <summary>
    /// Selects Question Marks' proxy effect using the combat RNG. The pool is deliberately the
    /// complete eligible effect catalogue, rather than this run's candidate-effect pool.
    /// </summary>
    internal static bool TrySelectRandomEffect(Player player, string excludedCardId, out string selectedEffectId)
    {
        string[] candidates = PillPoolCatalog.EffectPillIds
            .Where(id => !string.Equals(id, excludedCardId, StringComparison.Ordinal))
            .Where(PillPoolService.IsEligibleEffectPill)
            .ToArray();
        if (candidates.Length == 0)
        {
            selectedEffectId = string.Empty;
            return false;
        }

        selectedEffectId = candidates[player.RunState.Rng.CombatCardGeneration.NextInt(candidates.Length)];
        return true;
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

        // Transforming a Hand card which is already queued for a manual play makes the base
        // game's Transform command cancel that queued play and force the NCard back to Hand.
        // Move it to Play first, exactly as the mystery-reveal path does, so the replacement
        // stays in the active play flow regardless of how long its own effect animation lasts.
        await MoveQuestionMarksToPlayPile(questionMarks, isAutoPlay, skipCardPileVisuals);
        NCard? playNode = NCard.FindOnTable(questionMarks);

        CardModel canonicalEffect = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), selectedEffectId));
        ICardScope scope = questionMarks.CardScope
            ?? throw new InvalidOperationException("InstantPill cannot trigger Question Marks outside a card scope.");
        CardModel replacement = scope.CreateCard(canonicalEffect, questionMarks.Owner);
        replacement.DeckVersion = questionMarks.DeckVersion;
        Player owner = questionMarks.Owner;

        CardPileAddResult? transformResult = await CardCmd.Transform(
            questionMarks,
            replacement,
            CardPreviewStyle.None);
        if (transformResult is not { success: true })
        {
            throw new InvalidOperationException("InstantPill failed to substitute Question Marks with its selected effect.");
        }

        // A Play-pile transformation intentionally has no built-in card-node swap. Keep the
        // existing clicked card node and make it represent the selected effect before cleanup.
        if (playNode != null)
        {
            playNode.Model = transformResult.Value.cardAdded;
            playNode.UpdateVisuals(PileType.Play, CardPreviewMode.Normal);
        }

        // The replacement executes normally, including gameplay VFX. Its custom card audio is
        // muted for this async flow so Question Marks remains the sole authored voice cue.
        using (PillAudio.SuppressCustomCardSounds())
        using (CapsuleUsageHistory.SuppressProxyPlayHistory())
        {
            await transformResult.Value.cardAdded.OnPlayWrapper(
                choiceContext,
                target,
                isAutoPlay,
                resources,
                skipCardPileVisuals);
        }

        CapsuleUsageHistory.Record(owner, QuestionMarks.CardId);
    }

    private static async Task MoveQuestionMarksToPlayPile(
        CardModel questionMarks,
        bool isAutoPlay,
        bool skipCardPileVisuals)
    {
        if (!isAutoPlay)
        {
            await CardPileCmd.AddDuringManualCardPlay(questionMarks);
            return;
        }

        await CardPileCmd.Add(
            questionMarks,
            PileType.Play,
            CardPilePosition.Bottom,
            clonedBy: null,
            skipVisuals: skipCardPileVisuals);

        if (!skipCardPileVisuals)
        {
            await Cmd.CustomScaledWait(0.25f, 0.35f);
        }
    }
}
