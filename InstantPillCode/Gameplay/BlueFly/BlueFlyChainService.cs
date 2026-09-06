using System.Linq;
using System.Threading.Tasks;
using BlueFlyCard = InstantPill.InstantPillCode.Cards.Generated.BlueFly;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace InstantPill.InstantPillCode.Gameplay.BlueFly;

/// <summary>
/// Resolves the single dynamic Blue Fly chain launched by a manually played Blue Fly. The hand is
/// re-read after each autoplay, so flies drawn by earlier flies join the same chain naturally.
/// </summary>
internal static class BlueFlyChainService
{
    public static async Task PlayHandFlies(PlayerChoiceContext choiceContext, BlueFlyCard rootFly, Creature? initialTarget)
    {
        Creature? preferredTarget = initialTarget;

        while (!CombatManager.Instance.IsOverOrEnding && rootFly.Owner.Creature.IsAlive)
        {
            BlueFlyCard? nextFly = rootFly.Owner.PlayerCombatState?.Hand.Cards
                .OfType<BlueFlyCard>()
                .FirstOrDefault();
            if (nextFly is null)
            {
                return;
            }

            Creature? target = preferredTarget?.IsAlive == true
                ? preferredTarget
                : SelectRandomHittableEnemy(rootFly);
            if (target is null)
            {
                return;
            }

            await CardCmd.AutoPlay(choiceContext, nextFly, target);
        }
    }

    private static Creature? SelectRandomHittableEnemy(BlueFlyCard fly)
    {
        Creature[] enemies = fly.CombatState?.HittableEnemies.ToArray() ?? [];
        return enemies.Length == 0
            ? null
            : fly.Owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }
}
