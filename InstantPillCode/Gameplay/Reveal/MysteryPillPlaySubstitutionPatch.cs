using System;
using System.Threading.Tasks;
using HarmonyLib;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Content;
using InstantPill.InstantPillCode.Gameplay.Effects;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace InstantPill.InstantPillCode.Gameplay.Reveal;

/// <summary>
/// Substitutes an in-pool mystery pill with its already-assigned effect before the mystery card's
/// normal play wrapper creates a CardPlay. The effect card is therefore the sole played card.
/// </summary>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
internal static class MysteryPillPlaySubstitutionPatch
{
    [HarmonyPrefix]
    private static bool SubstituteMysteryPill(
        CardModel __instance,
        ref Task __result,
        PlayerChoiceContext choiceContext,
        Creature? target,
        bool isAutoPlay,
        ResourceInfo resources,
        bool skipCardPileVisuals)
    {
        if (!__instance.Keywords.Contains(InstantPillKeywords.Mystery))
        {
            return true;
        }

        string? effectCardId = PillPoolService.TryGetAssignedEffectCardId(__instance.Owner, __instance.Id.Entry);
        if (effectCardId == null)
        {
            // The base mystery card will show its no-effect thought bubble.
            return true;
        }

        __result = TransformAndPlayEffect(
            __instance,
            effectCardId,
            choiceContext,
            target,
            isAutoPlay,
            resources,
            skipCardPileVisuals);
        return false;
    }

    private static async Task TransformAndPlayEffect(
        CardModel mysteryCard,
        string effectCardId,
        PlayerChoiceContext choiceContext,
        Creature? target,
        bool isAutoPlay,
        ResourceInfo resources,
        bool skipCardPileVisuals)
    {
        // Place the selected mystery card in Play before transforming it. CardCmd.Transform has
        // special cancellation logic for a Hand card that is still queued for play; transforming
        // after this step keeps the replacement in Play and prevents the visible hand round-trip.
        await MoveMysteryToPlayPile(mysteryCard, isAutoPlay, skipCardPileVisuals);
        NCard? playNode = NCard.FindOnTable(mysteryCard);

        // CardCmd.Transform removes the original CardModel from its scope. Keep the identity
        // needed for the saved mapping before that happens.
        var owner = mysteryCard.Owner;
        string mysteryCardId = mysteryCard.Id.Entry;
        string playedEffectCardId = effectCardId;
        bool isQuestionMarks = string.Equals(effectCardId, QuestionMarks.CardId, StringComparison.Ordinal);
        if (isQuestionMarks)
        {
            if (!QuestionMarksPlaySubstitutionPatch.TrySelectRandomEffect(
                    owner,
                    QuestionMarks.CardId,
                    out playedEffectCardId))
            {
                throw new InvalidOperationException("InstantPill could not find a proxy effect for Question Marks.");
            }

            if (LocalContext.IsMine(mysteryCard))
            {
                QuestionMarks.PlayRandomSound();
            }
        }

        CardModel effectCard = await PillRevealTransformer.TransformAllCopies(
            owner,
            mysteryCard,
            effectCardId,
            playedEffectCardId);

        // Transforming a Play-pile model deliberately has no built-in on-table transform VFX.
        // Rebind the existing card node so the same card that was clicked immediately displays
        // the revealed effect and can receive that effect pill's consume VFX during cleanup.
        if (playNode != null)
        {
            playNode.Model = effectCard;
            playNode.UpdateVisuals(PileType.Play, CardPreviewMode.Normal);
        }

        string? activatedEffectId = PillPoolService.ActivateMysteryPill(owner, mysteryCardId);
        if (activatedEffectId != effectCardId)
        {
            throw new System.InvalidOperationException(
                $"InstantPill activated {activatedEffectId ?? "no effect"} after preparing {effectCardId}.");
        }

        if (isQuestionMarks)
        {
            using (PillAudio.SuppressCustomCardSounds())
            using (CapsuleUsageHistory.SuppressProxyPlayHistory())
            {
                await effectCard.OnPlayWrapper(
                    choiceContext,
                    target,
                    isAutoPlay,
                    resources,
                    skipCardPileVisuals);
            }
            CapsuleUsageHistory.Record(owner, QuestionMarks.CardId);
            return;
        }

        await effectCard.OnPlayWrapper(
            choiceContext,
            target,
            isAutoPlay,
            resources,
            skipCardPileVisuals);
    }

    private static async Task MoveMysteryToPlayPile(
        CardModel mysteryCard,
        bool isAutoPlay,
        bool skipCardPileVisuals)
    {
        if (!isAutoPlay)
        {
            await CardPileCmd.AddDuringManualCardPlay(mysteryCard);
            return;
        }

        await CardPileCmd.Add(
            mysteryCard,
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
