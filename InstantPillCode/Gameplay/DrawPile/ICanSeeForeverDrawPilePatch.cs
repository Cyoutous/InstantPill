using System;
using System.Linq;
using HarmonyLib;
using InstantPill.InstantPillCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace InstantPill.InstantPillCode.Gameplay.DrawPile;

/// <summary>
/// The base game sorts cards in the draw-pile viewer by rarity and ID. When I Can See Forever is
/// present, replace only that view with the CardPile's own order; CardPile.Cards[0] remains the
/// card that the normal draw command will draw next.
/// </summary>
[HarmonyPatch(typeof(NCardPileScreen), "OnPileContentsChanged")]
internal static class ICanSeeForeverDrawPilePatch
{
    private static readonly AccessTools.FieldRef<NCardPileScreen, NCardGrid> GridField =
        AccessTools.FieldRefAccess<NCardPileScreen, NCardGrid>("_grid");

    [HarmonyPrefix]
    private static bool PreserveActualDrawOrder(NCardPileScreen __instance)
    {
        CardPile pile = __instance.Pile;
        if (pile.Type != PileType.Draw || !OwnerCanSeeDrawOrder(pile))
        {
            return true;
        }

        try
        {
            // Do not mutate Pile.Cards: this is solely the card-grid's presentation list.
            GridField(__instance).SetCards(
                pile.Cards.ToList(),
                pile.Type,
                [SortingOrders.Ascending]);
            return false;
        }
        catch (Exception exception)
        {
            // Fall back to the unmodified viewer if a future game version changes its UI field.
            MainFile.Logger.Warn($"I Can See Forever could not preserve draw-pile order: {exception.Message}", 1);
            return true;
        }
    }

    private static bool OwnerCanSeeDrawOrder(CardPile pile)
    {
        CardModel? representativeCard = pile.Cards.FirstOrDefault();
        return representativeCard?.Owner?.Creature.HasPower<ICanSeeForeverPower>() == true;
    }
}
