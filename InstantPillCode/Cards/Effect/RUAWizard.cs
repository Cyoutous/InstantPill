using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>A grade-1 pill that transforms one other card in the owner's hand.</summary>
[CustomID("INSTANTPILL-R_U_A_WIZARD")]
public sealed class RUAWizard : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/r u a wiz 2.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        IReadOnlyList<CardModel> transformableHandCards = Owner.PlayerCombatState?.Hand.Cards
            .Where(IsEligibleForTransform)
            .ToList() ?? [];

        if (transformableHandCards.Count == 0)
        {
            return;
        }

        if (transformableHandCards.Count == 1)
        {
            await Transform(transformableHandCards[0]);
            return;
        }

        CardModel? selectedCard = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1),
            IsEligibleForTransform,
            this)).FirstOrDefault();
        if (selectedCard is not null)
        {
            await Transform(selectedCard);
        }
    }

    private bool IsEligibleForTransform(CardModel card) => card != this && card.IsTransformable;

    private async Task Transform(CardModel card)
    {
        await CardCmd.TransformToRandom(card, Owner.RunState.Rng.CombatCardGeneration);
    }
}
