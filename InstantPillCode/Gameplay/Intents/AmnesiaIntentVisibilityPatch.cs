using System.Linq;
using HarmonyLib;
using InstantPill.InstantPillCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace InstantPill.InstantPillCode.Gameplay.Intents;

/// <summary>
/// Keeps enemy intents out of the local player's view while that player is Amnesiac.
/// This is deliberately a presentation-only patch: it never changes a monster's NextMove.
/// </summary>
[HarmonyPatch(typeof(NIntent), nameof(NIntent.UpdateIntent))]
internal static class AmnesiaIntentVisibilityPatch
{
    [HarmonyPostfix]
    private static void ApplyVisibility(NIntent __instance, Creature owner)
    {
        __instance.Visible = !ShouldHideIntents(owner.CombatState);
    }

    internal static void Refresh(ICombatState? combatState)
    {
        if (combatState is null || NCombatRoom.Instance is not { } combatRoom)
        {
            return;
        }

        bool shouldShow = !ShouldHideIntents(combatState);
        foreach (Creature enemy in combatState.Enemies)
        {
            NCreature? creatureNode = combatRoom.GetCreatureNode(enemy);
            if (creatureNode is null)
            {
                continue;
            }

            foreach (NIntent intent in creatureNode.IntentContainer.GetChildren(false).OfType<NIntent>())
            {
                intent.Visible = shouldShow;
            }
        }
    }

    private static bool ShouldHideIntents(ICombatState? combatState)
    {
        return LocalContext.GetMe(combatState)?.Creature.HasPower<AmnesiaPower>() == true;
    }
}
