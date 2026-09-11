using System;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// False PHD's excluded Experimental Pill result. It uses the original negative-effect table
/// and ranges, while deliberately omitting Experimental Pill's positive roll.
/// </summary>
[CustomID("INSTANTPILL-FPHD_EXPERIMENTAL_PILL")]
public sealed class FphdExperimentalPill : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/experimental pill_1.wav";
    private const float SoundVolume = 1f;

    private enum NegativeEffect
    {
        LoseStrength,
        LoseDexterity,
        LoseFocus,
        Weak,
        Frail,
        Vulnerable,
        NoBlock,
        NoDraw,
        NoEnergyGain,
        Confused,
        Slimed,
        Burn,
        Wound,
        Dazed
    }

    public FphdExperimentalPill() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        NegativeEffect negative = (NegativeEffect)Owner.RunState.Rng.CombatCardSelection
            .NextInt(Enum.GetValues<NegativeEffect>().Length);
        await ApplyNegativeEffect(choiceContext, cardPlay, negative);
    }

    private async Task ApplyNegativeEffect(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        NegativeEffect effect)
    {
        switch (effect)
        {
            case NegativeEffect.LoseStrength: await ApplyPower<StrengthPower>(choiceContext, cardPlay, -1m); break;
            case NegativeEffect.LoseDexterity: await ApplyPower<DexterityPower>(choiceContext, cardPlay, -1m); break;
            case NegativeEffect.LoseFocus: await ApplyPower<FocusPower>(choiceContext, cardPlay, -1m); break;
            case NegativeEffect.Weak: await ApplyPower<WeakPower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case NegativeEffect.Frail: await ApplyPower<FrailPower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case NegativeEffect.Vulnerable: await ApplyPower<VulnerablePower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case NegativeEffect.NoBlock: await ApplyPower<NoBlockPower>(choiceContext, cardPlay, 1m); break;
            case NegativeEffect.NoDraw: await ApplyPower<NoDrawPower>(choiceContext, cardPlay, 1m); break;
            case NegativeEffect.NoEnergyGain: await ApplyPower<NoEnergyGainPower>(choiceContext, cardPlay, 1m); break;
            case NegativeEffect.Confused: await ApplyPower<ConfusedPower>(choiceContext, cardPlay, 1m); break;
            case NegativeEffect.Slimed: await AddGeneratedCardsToHand<Slimed>(2); break;
            case NegativeEffect.Burn: await AddGeneratedCardToHand<Burn>(); break;
            case NegativeEffect.Wound: await AddGeneratedCardToHand<Wound>(); break;
            case NegativeEffect.Dazed: await AddGeneratedCardsToDiscard<Dazed>(RollInclusive(2, 3)); break;
            default: throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
        }
    }

    private async Task ApplyPower<TPower>(PlayerChoiceContext choiceContext, CardPlay cardPlay, decimal amount)
        where TPower : PowerModel, new() =>
        await PowerCmd.Apply<TPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);

    private int RollInclusive(int minimum, int maximum) =>
        minimum + Owner.RunState.Rng.CombatCardSelection.NextInt(maximum - minimum + 1);

    private async Task AddGeneratedCardToHand<TCard>() where TCard : CardModel
    {
        TCard card = CombatState!.CreateCard<TCard>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }

    private async Task AddGeneratedCardsToHand<TCard>(int count) where TCard : CardModel
    {
        for (int index = 0; index < count; index++)
        {
            await AddGeneratedCardToHand<TCard>();
        }
    }

    private async Task AddGeneratedCardsToDiscard<TCard>(int count) where TCard : CardModel
    {
        for (int index = 0; index < count; index++)
        {
            TCard card = CombatState!.CreateCard<TCard>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, Owner);
        }
    }
}
