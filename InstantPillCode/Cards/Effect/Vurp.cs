using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Gameplay.Effects;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// Recreates the most recently used non-Vurp capsule as a combat-only generated card.
/// When no such capsule exists yet, it draws a current identity from the player's capsule pool.
/// </summary>
[CustomID(CardId)]
public sealed class Vurp : BaseEffectPillCard
{
    public const string CardId = "INSTANTPILL-VURP";

    private const string SoundPath = "res://audio/vurp 1.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        if (CombatState == null)
        {
            return;
        }

        CardModel capsule = CapsuleUsageHistory.CreateLastOrRandomCapsule(CombatState, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(capsule, PileType.Hand, Owner);
    }
}
