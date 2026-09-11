using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using Godot;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-1 pill that damages and poisons every living enemy.</summary>
[CustomID("INSTANTPILL-BAD_GAS")]
public sealed class BadGas : BaseEffectPillCard
{
    private static readonly Color PoisonVfxTint = new("83eb85");

    private const string SoundPath = "res://audio/bad gas 2.wav";
    private const float SoundVolume = 1f;
    private const decimal Damage = 2m;
    private const decimal Poison = 2m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-HEALTH_DOWN";

    // Uses the original Poison power's localized title and description in the normal card hover UI.
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        await DamageCmd.Attack(Damage)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState!)
            .Execute(choiceContext);

        // Copy the current list: applying a power can update combat UI/state while this loop runs.
        IReadOnlyList<Creature> survivingEnemies = CombatState!.HittableEnemies.ToList();
        await PlayNoxiousFumesVfx(survivingEnemies);
        await PowerCmd.Apply<PoisonPower>(choiceContext, survivingEnemies, Poison, Owner.Creature, this);
    }

    /// <summary>
    /// Mirrors Noxious Fumes: every target receives its gaseous poison impact before
    /// the group poison command resolves, making the impacts appear together.
    /// </summary>
    private static async Task PlayNoxiousFumesVfx(IReadOnlyList<Creature> enemies)
    {
        await Cmd.CustomScaledWait(0.2f, 0.4f);

        NCombatRoom? combatRoom = NCombatRoom.Instance;
        if (combatRoom is null)
        {
            return;
        }

        foreach (Creature enemy in enemies)
        {
            NCreature? targetNode = combatRoom.GetCreatureNode(enemy);
            if (targetNode is not null)
            {
                NGaseousImpactVfx? gaseousImpact = NGaseousImpactVfx.Create(targetNode.VfxSpawnPosition, PoisonVfxTint);
                if (gaseousImpact is not null)
                {
                    combatRoom.CombatVfxContainer.AddChildSafely(gaseousImpact);
                }
            }
        }
    }
}
