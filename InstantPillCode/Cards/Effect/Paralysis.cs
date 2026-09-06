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

/// <summary>A grade-1 pill that discards one remaining card from the owner's hand.</summary>
[CustomID("INSTANTPILL-PARALYSIS")]
public sealed class Paralysis : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/paralysis 3.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade1;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        IReadOnlyList<CardModel> remainingHandCards = Owner.PlayerCombatState?.Hand.Cards
            .Where(card => card != this)
            .ToList() ?? [];

        if (remainingHandCards.Count == 0)
        {
            return;
        }

        if (remainingHandCards.Count == 1)
        {
            await CardCmd.Discard(choiceContext, remainingHandCards[0]);
            return;
        }

        CardModel? selectedCard = (await CardSelectCmd.FromHandForDiscard(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1),
            null,
            this)).FirstOrDefault();
        if (selectedCard is not null)
        {
            await CardCmd.Discard(choiceContext, selectedCard);
        }
    }
}
