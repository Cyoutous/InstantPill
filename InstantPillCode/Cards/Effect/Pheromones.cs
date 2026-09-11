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
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-3 pill that applies the original Stun effect to every hittable enemy.</summary>
[CustomID("INSTANTPILL-PHEROMONES")]
public sealed class Pheromones : BaseEffectPillCard
{
    private static readonly Color VulnerablePotionVfxTint = new("f6bfd0");

    private const string SoundPath = "res://audio/pheromones 2.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-PARALYSIS";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [StunIntent.GetStaticHoverTip()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        IReadOnlyList<Creature> enemiesToStun = CombatState!.HittableEnemies.ToList();
        PlayVulnerablePotionVfx(enemiesToStun);
        foreach (Creature enemy in enemiesToStun)
        {
            await CreatureCmd.Stun(enemy);
        }
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
