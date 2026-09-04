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
    // The shared test base still requires this member; this card overrides its test Strength play effect.
    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, 1m);

        // Thought bubbles are local visual effects. The owning client creates exactly one bubble
        // anchored to the player who actually played this card.
        if (!LocalContext.IsMine(this))
        {
            return;
        }

        PillAudio.PlayIFoundPills();

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
