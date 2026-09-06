using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-2 pill that applies Snecko Oil's temporary cost randomization to the current hand,
/// without its card draw.
/// </summary>
[CustomID("INSTANTPILL-AMNESIA")]
public sealed class Amnesia : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/amnesia 2.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards.Where(card => !card.EnergyCost.CostsX))
        {
            // Match Snecko Oil: only cards with a normal payable cost receive a temporary value.
            if (card.EnergyCost.GetWithModifiers(CostModifiers.None) < 0m)
            {
                continue;
            }

            card.EnergyCost.SetThisTurnOrUntilPlayed(Owner.RunState.Rng.CombatEnergyCosts.NextInt(4));
            NCard.FindOnTable(card)?.PlayRandomizeCostAnim();
        }

        return Task.CompletedTask;
    }
}
