using System.Threading.Tasks;
using HarmonyLib;
using InstantPill.InstantPillCode.Content;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

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
        // CardCmd.Transform removes the original CardModel from its scope. Keep the identity
        // needed for the saved mapping before that happens.
        var owner = mysteryCard.Owner;
        string mysteryCardId = mysteryCard.Id.Entry;

        CardModel effectCard = await PillRevealTransformer.TransformAllCopies(
            owner,
            mysteryCard,
            effectCardId);

        string? activatedEffectId = PillPoolService.ActivateMysteryPill(owner, mysteryCardId);
        if (activatedEffectId != effectCardId)
        {
            throw new System.InvalidOperationException(
                $"InstantPill activated {activatedEffectId ?? "no effect"} after preparing {effectCardId}.");
        }

        await effectCard.OnPlayWrapper(
            choiceContext,
            target,
            isAutoPlay,
            resources,
            skipCardPileVisuals);
    }
}
