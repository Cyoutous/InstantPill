using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using Godot;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-2 pill that hits one random enemy, then transfers twice that resolved damage to every
/// other hittable enemy. The transfer follows Omnislice's result-based damage propagation.
/// </summary>
[CustomID("INSTANTPILL-HURF")]
public sealed class Hurf : BaseEffectPillCard
{
    private static readonly Color PoisonVfxTint = new("83eb85");

    private const string SoundPath = "res://audio/horf 2.wav";
    private const float SoundVolume = 1f;
    private const decimal InitialDamage = 10m;
    private const decimal SecondaryDamageMultiplier = 2m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        IReadOnlyList<Creature>? enemies = CombatState?.HittableEnemies.ToList();
        if (enemies is not { Count: > 0 })
        {
            return;
        }

        Creature? initialTarget = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
        if (initialTarget is null)
        {
            return;
        }

        await using AttackContext context = await AttackCommand.CreateContextAsync(CombatState!, choiceContext, cardPlay);
        PlayNoxiousFumesVfx([initialTarget]);

        List<DamageResult> initialResults = (await CreatureCmd.Damage(
            choiceContext,
            initialTarget,
            InitialDamage,
            ValueProp.Move,
            this,
            cardPlay)).ToList();
        context.AddHit(initialResults);

        DamageResult? initialResult = initialResults.FirstOrDefault();
        if (initialResult is null)
        {
            return;
        }

        // This is the same resolved-damage basis used by Omnislice. Applying Unpowered prevents
        // Strength and other powered-attack modifiers from changing the propagated hit a second time.
        decimal secondaryDamage = (initialResult.TotalDamage + initialResult.OverkillDamage)
            * SecondaryDamageMultiplier;
        List<Creature> otherEnemies = CombatState!.GetTeammatesOf(initialResult.Receiver)
            .Except([initialTarget])
            .Where(enemy => enemy.IsHittable)
            .ToList();

        if (secondaryDamage <= 0m || otherEnemies.Count == 0)
        {
            return;
        }

        PlayNoxiousFumesVfx(otherEnemies);
        context.AddHit(await CreatureCmd.Damage(
            choiceContext,
            otherEnemies,
            secondaryDamage,
            ValueProp.Unpowered | ValueProp.Move,
            Owner.Creature,
            this,
            cardPlay));
    }

    /// <summary>
    /// Uses the original Noxious Fumes gaseous impact only as a hit visual. It does not create
    /// Poison or alter the damage command that follows it.
    /// </summary>
    private static void PlayNoxiousFumesVfx(IEnumerable<Creature> enemies)
    {
        NCombatRoom? combatRoom = NCombatRoom.Instance;
        if (combatRoom is null)
        {
            return;
        }

        foreach (Creature enemy in enemies)
        {
            NCreature? targetNode = combatRoom.GetCreatureNode(enemy);
            if (targetNode is null)
            {
                continue;
            }

            NGaseousImpactVfx? gaseousImpact = NGaseousImpactVfx.Create(targetNode.VfxSpawnPosition, PoisonVfxTint);
            if (gaseousImpact is not null)
            {
                combatRoom.CombatVfxContainer.AddChildSafely(gaseousImpact);
            }
        }
    }
}
