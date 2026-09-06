using System;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Gameplay.Effects;

/// <summary>
/// Stores each player's most recently used non-Vurp capsule for the current run.
/// A single value is sufficient: Vurp never overwrites it, so consecutive Vurps naturally
/// continue to refer to the prior non-Vurp capsule without needing a history stack.
/// </summary>
public static class CapsuleUsageHistory
{
    public static SavedSpireField<Player, string> LastNonVurpCapsuleId { get; } =
        new(_ => string.Empty, "instant_pill_last_non_vurp_capsule");

    public static void Record(CardModel card)
    {
        if (!card.Keywords.Contains(InstantPillKeywords.Capsule)
            || string.Equals(card.Id.Entry, Vurp.CardId, StringComparison.Ordinal))
        {
            return;
        }

        LastNonVurpCapsuleId.Set(card.Owner, card.Id.Entry);
        MainFile.Logger.Info(
            $"Recorded {card.Id.Entry} as the last non-Vurp capsule for player {card.Owner.NetId}.",
            1);
    }

    /// <summary>
    /// Produces a combat-only copy of the last recorded capsule. If no capsule has been recorded,
    /// rolls the player's existing capsule-pool slots instead. The caller adds the result through
    /// AddGeneratedCardToCombat, so it never enters the permanent deck.
    /// </summary>
    public static CardModel CreateLastOrRandomCapsule(ICombatState combatState, Player player)
    {
        string cardId = LastNonVurpCapsuleId.Get(player) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(cardId))
        {
            cardId = Pools.PillPoolService.RollCapsuleCardIdExcluding(player, Vurp.CardId);
        }

        CardModel canonicalCard = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), cardId));
        return combatState.CreateCard(canonicalCard, player);
    }
}

/// <summary>Registers Vurp's primitive per-player capsule-history field with BaseLib saves.</summary>
public static class CapsuleUsageSaveRegistration
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        if (!CapsuleUsageHistory.LastNonVurpCapsuleId.RegisterCustomSave())
        {
            throw new InvalidOperationException("InstantPill could not register the Vurp capsule-history save field.");
        }

        _registered = true;
    }
}

/// <summary>
/// Records the actual card that completed its play. Mystery pills have already been transformed
/// into their revealed effect card at this hook, so Vurp reproduces the revealed result rather
/// than the old mystery identity.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
internal static class CapsuleUsageHistoryPatch
{
    [HarmonyPostfix]
    private static void RecordPlayedCapsule(ref Task __result, CardPlay cardPlay)
    {
        __result = RecordAfterCardPlay(__result, cardPlay);
    }

    private static async Task RecordAfterCardPlay(Task originalTask, CardPlay cardPlay)
    {
        await originalTask;
        CapsuleUsageHistory.Record(cardPlay.Card);
    }
}
