using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using HarmonyLib;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Gameplay;

/// <summary>
/// Implements the non-display behaviour owned by the InstantPill Capsule keyword.
/// This intentionally uses the transient retain flag, rather than CardKeyword.Retain,
/// so a capsule presents one concise tooltip instead of three separate keywords.
/// </summary>
[HarmonyPatch]
internal static class CapsuleKeywordBehaviorPatch
{
    /// <summary>
    /// The game's flush decision happens immediately after Hook.BeforeFlush completes.
    /// Marking each capsule with one-turn retain here therefore preserves only those
    /// individual cards, without retaining the whole hand.
    /// </summary>
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeFlush))]
    [HarmonyPostfix]
    private static void RetainCapsulesBeforeHandFlush(ref Task __result, Player player)
    {
        __result = GiveCapsulesSingleTurnRetain(__result, player);
    }

    private static async Task GiveCapsulesSingleTurnRetain(Task originalTask, Player player)
    {
        await originalTask;

        CardPile? hand = CardPile.Get(PileType.Hand, player);
        if (hand == null)
        {
            return;
        }

        foreach (CardModel card in hand.Cards.Where(IsCapsule))
        {
            card.GiveSingleTurnRetain();
        }
    }

    /// <summary>
    /// A successful card play has finished only after OnPlayWrapper's task completes.
    /// Remove the persistent deck version at that point. A combat-only generated card has no
    /// DeckVersion and is already handled by the base game's normal pile cleanup.
    /// </summary>
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    [HarmonyPostfix]
    private static void RemovePlayedCapsuleFromDeck(ref Task __result, CardModel __instance)
    {
        __result = RemoveDeckVersionAfterSuccessfulPlay(__result, __instance);
    }

    private static async Task RemoveDeckVersionAfterSuccessfulPlay(Task originalTask, CardModel playedCard)
    {
        await originalTask;

        if (!IsCapsule(playedCard))
        {
            return;
        }

        CardModel? deckVersion = playedCard.DeckVersion;
        if (deckVersion?.Pile?.Type != PileType.Deck)
        {
            return;
        }

        await CardPileCmd.RemoveFromDeck(deckVersion, showPreview: false);
        MainFile.Logger.Info($"Capsule removed {deckVersion.Id.Entry} from the player's deck after play.", 1);
    }

    internal static bool IsCapsule(CardModel card) =>
        card.Keywords.Contains(InstantPillKeywords.Capsule);
}

/// <summary>
/// Only identified effect pills replace the native Power-card flight. Mystery pills remain
/// ordinary Power cards until their reveal flow starts an identified effect card's play wrapper.
/// This deliberately patches the VFX method itself instead of rewriting OnPlayWrapper's
/// <c>Type == CardType.Power</c> condition. That condition belongs to every Power card, so
/// replacing its Type getter risks changing the native branch for non-mod cards when the
/// game's generated IL changes between beta builds.
/// </summary>
[HarmonyPatch(typeof(CardModel), "PlayPowerCardFlyVfx")]
internal static class EffectPillPowerPlayVfxPatch
{
    [HarmonyPrefix]
    private static bool SkipPowerFlyVfxForEffectPills(CardModel __instance, ref Task __result)
    {
        if (!UsesEffectPillConsumeVfx(__instance))
        {
            return true;
        }

        __result = Task.CompletedTask;
        return false;
    }

    internal static bool UsesEffectPillConsumeVfx(CardModel card) =>
        card is BaseEffectPillCard effectPill && effectPill.UsesEffectPillConsumeVfx;
}

/// <summary>
/// The native combat-removal path intentionally suppresses its consume visual for played Power
/// cards. At that visual-only branch, present identified effect pills as Skills so the existing
/// NExhaustVfx path runs. Their actual CardType and all gameplay interactions remain Power.
/// </summary>
[HarmonyPatch]
internal static class EffectPillConsumeVfxPatch
{
    private static MethodBase? TargetMethod() => AccessTools.AsyncMoveNext(
        AccessTools.Method(
            typeof(CardPileCmd),
            nameof(CardPileCmd.RemoveFromCombat),
            [typeof(IEnumerable<CardModel>), typeof(bool)]));

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> EnableConsumeVfxForEffectPills(
        IEnumerable<CodeInstruction> instructions)
    {
        MethodInfo typeGetter = AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.Type));
        MethodInfo replacement = AccessTools.Method(typeof(EffectPillConsumeVfxPatch), nameof(GetRemovalVisualType));
        bool replaced = false;

        foreach (CodeInstruction instruction in instructions)
        {
            if (!replaced && instruction.Calls(typeGetter))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = replacement;
                replaced = true;
            }

            yield return instruction;
        }

        if (!replaced)
        {
            MainFile.Logger.Error("InstantPill could not locate CardPileCmd.RemoveFromCombat's Power consume-VFX check.", 1);
        }
    }

    private static CardType GetRemovalVisualType(CardModel card) =>
        EffectPillPowerPlayVfxPatch.UsesEffectPillConsumeVfx(card) ? CardType.Skill : card.Type;
}
