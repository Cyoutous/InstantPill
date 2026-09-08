using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace InstantPill.InstantPillCode.Cards;

/// <summary>
/// BaseLib exposes a custom full-card frame directly. These remaining visual
/// layers are separate CardModel properties in the base game, so override them
/// only for cards derived from <see cref="BasePillCard"/>.
/// </summary>
[HarmonyPatch]
internal static class PillCardVisualOverrides
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.PortraitBorder), MethodType.Getter)]
    private static bool UsePillPortraitBorder(CardModel __instance, ref Texture2D? __result)
    {
        if (__instance is not BasePillCard)
            return true;

        __result = BasePillCard.PillPortraitBorder;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.BannerTexture), MethodType.Getter)]
    private static bool UsePillBanner(CardModel __instance, ref Texture2D? __result)
    {
        if (__instance is not BasePillCard)
            return true;

        __result = BasePillCard.PillBanner;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.EnergyIcon), MethodType.Getter)]
    private static bool UsePillEnergyIcon(CardModel __instance, ref Texture2D? __result)
    {
        if (__instance is not BasePillCard)
            return true;

        __result = BasePillCard.PillEnergyIcon;
        return false;
    }

    /// <summary>
    /// RitsuLib can apply card visual-style fixes after CardModel's properties
    /// have been read. Reapply these three independent UI layers after every
    /// NCard reload so the pill art remains authoritative with or without it.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(NCard), "Reload")]
    private static void ReapplyPillVisualNodes(NCard __instance)
    {
        ApplyPillVisualNodes(__instance);
    }

    /// <summary>
    /// Preview updates call UpdatePortrait again after Reload. That native method
    /// resets the portrait border and title banner, but not the energy icon.
    /// Reapply the pill layers after every such refresh as well.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(NCard), "UpdatePortrait")]
    private static void ReapplyPillVisualNodesAfterPortraitUpdate(NCard __instance)
    {
        ApplyPillVisualNodes(__instance);
    }

    private static void ApplyPillVisualNodes(NCard __instance)
    {
        if (__instance.Model is not BasePillCard)
            return;

        TextureRect? portraitBorder = __instance.GetNodeOrNull<TextureRect>("%PortraitBorder");
        if (portraitBorder != null)
        {
            portraitBorder.Texture = BasePillCard.PillPortraitBorder;
            portraitBorder.Material = BasePillCard.PillUiMaterial;
        }

        TextureRect? banner = __instance.GetNodeOrNull<TextureRect>("%TitleBanner");
        if (banner != null)
        {
            banner.Texture = BasePillCard.PillBanner;
            banner.Material = BasePillCard.PillUiMaterial;
        }

        TextureRect? energyIcon = __instance.GetNodeOrNull<TextureRect>("%EnergyIcon");
        if (energyIcon != null)
        {
            energyIcon.Texture = BasePillCard.PillEnergyIcon;
            energyIcon.Material = BasePillCard.PillUiMaterial;
        }
    }
}
