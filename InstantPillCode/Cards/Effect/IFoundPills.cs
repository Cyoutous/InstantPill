using System;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-0 pill that heals its actual player-owner and reacts in that character's voice.</summary>
[CustomID("INSTANTPILL-I_FOUND_PILLS")]
public sealed class IFoundPills : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/i found pills 3.wav";
    private const float SoundVolume = 0.6f;
    // These are native card-effect sounds, distinct from the authored voice line above. They
    // deliberately remain audible when Question Marks proxies I Found Pills.
    private static readonly string[] ChantSfxPaths =
    [
        "event:/sfx/enemy/enemy_attacks/devoted_sculptor/devoted_sculptor_cast",
        "event:/sfx/enemy/enemy_attacks/cultists/cultists_buff_calcified",
        "event:/sfx/enemy/enemy_attacks/cultists/cultists_buff_damp"
    ];

    // The shared test base still requires this member; this card overrides its test Strength play effect.
    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Reuse the native character PowerUp animation path used by original Power cards.
        // Unlike the local voice/audio presentation below, this command is part of the card's
        // gameplay presentation and therefore remains visible to every combat participant.
        await CreatureCmd.TriggerAnim(
            Owner.Creature,
            "PowerUp",
            Owner.Character.PowerUpAnimDelay);

        await CreatureCmd.Heal(Owner.Creature, 1m);

        // Thought bubbles are local visual effects. The owning client creates exactly one bubble
        // anchored to the player who actually played this card.
        if (!LocalContext.IsMine(this))
        {
            return;
        }

        PillAudio.PlayOneShot(SoundPath, SoundVolume);
        PillAudio.PlayVanillaCardEffect(ChantSfxPaths[Random.Shared.Next(ChantSfxPaths.Length)]);

        string text = new LocString("combat_messages", GetThoughtKey()).GetFormattedText();
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(
            NThoughtBubbleVfx.Create(text, Owner.Creature, 1.5));
    }

    private string GetThoughtKey() => Owner.Character switch
    {
        Ironclad => "INSTANTPILL-I_FOUND_PILLS-THOUGHT-IRONCLAD",
        Silent => "INSTANTPILL-I_FOUND_PILLS-THOUGHT-SILENT",
        Regent => "INSTANTPILL-I_FOUND_PILLS-THOUGHT-REGENT",
        Necrobinder => "INSTANTPILL-I_FOUND_PILLS-THOUGHT-NECROBINDER",
        Defect => "INSTANTPILL-I_FOUND_PILLS-THOUGHT-DEFECT",
        _ => "INSTANTPILL-I_FOUND_PILLS-THOUGHT-UNKNOWN"
    };
}
