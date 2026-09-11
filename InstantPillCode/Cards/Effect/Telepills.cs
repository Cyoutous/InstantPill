using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-0 pill that creates one random card from the owner's character pool for this combat.
/// </summary>
[CustomID("INSTANTPILL-TELEPILLS")]
public sealed class Telepills : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/telepills 4.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override string? FalsePhdReplacementCardId => QuestionMarks.CardId;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        CardModel? generatedCard = CardFactory.GetDistinctForCombat(
                Owner,
                Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint),
                1,
                Owner.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();
        if (generatedCard is null)
        {
            return;
        }

        PileType targetPile = Owner.RunState.Rng.CombatCardGeneration.NextBool()
            ? PileType.Draw
            : PileType.Discard;
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(
            generatedCard,
            targetPile,
            Owner,
            CardPilePosition.Random));
    }
}
