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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-1 pill that applies two stacks of the original Demise power to all enemies.</summary>
[CustomID("INSTANTPILL-LEMON_PARTY")]
public sealed class LemonParty : BaseEffectPillCard
{
    private static readonly Color VulnerablePotionVfxTint = new("f7e628");

    private const string SoundPath = "res://audio/lemon party 1.wav";
    private const float SoundVolume = 1f;
    private const decimal DemiseAmount = 2m;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        IReadOnlyList<Creature> enemies = CombatState!.HittableEnemies.ToList();
        PlayVulnerablePotionVfx(enemies);
        await PowerCmd.Apply<DemisePower>(choiceContext, enemies, DemiseAmount, Owner.Creature, this);
    }

    /// <summary>
    /// Mirrors Vulnerable Potion's red splash only. No Vulnerable power is created or applied.
    /// </summary>
    private static void PlayVulnerablePotionVfx(IReadOnlyList<Creature> enemies)
    {
        NCombatRoom? combatRoom = NCombatRoom.Instance;
        if (combatRoom is null)
        {
            return;
        }

        foreach (Creature enemy in enemies)
        {
            combatRoom.PlaySplashVfx(enemy, VulnerablePotionVfxTint);
        }
    }
}
