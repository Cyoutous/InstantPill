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
/// A grade-2 pill that independently rolls one uniformly weighted effect from each of its
/// positive and negative tables. Numeric ranges use the same combat-selection RNG stream.
/// </summary>
[CustomID("INSTANTPILL-EXPERIMENTAL_PILL")]
public sealed class ExperimentalPill : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/experimental pill_1.wav";
    private const float SoundVolume = 1f;

    private enum PositiveEffect
    {
        Strength,
        Dexterity,
        Focus,
        Buffer,
        EnergyNextTurn,
        DrawNextTurn,
        Barricade,
        BlockNextTurn,
        Clarity,
        Intangible,
        Thorns,
        Vigor,
        FreeAttack,
        FreeSkill,
        FreePower,
        MaxHp,
        Coolheaded,
        Scrawl
    }

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

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    public override string? PhdReplacementCardId => "INSTANTPILL-PHD_EXPERIMENTAL_PILL";

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-FPHD_EXPERIMENTAL_PILL";

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        PositiveEffect positive = (PositiveEffect)Owner.RunState.Rng.CombatCardSelection
            .NextInt(Enum.GetValues<PositiveEffect>().Length);
        await ApplyPositiveEffect(choiceContext, cardPlay, positive);

        NegativeEffect negative = (NegativeEffect)Owner.RunState.Rng.CombatCardSelection
            .NextInt(Enum.GetValues<NegativeEffect>().Length);
        await ApplyNegativeEffect(choiceContext, cardPlay, negative);
    }

    private async Task ApplyPositiveEffect(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        PositiveEffect effect)
    {
        switch (effect)
        {
            case PositiveEffect.Strength:
                await ApplyPower<StrengthPower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case PositiveEffect.Dexterity:
                await ApplyPower<DexterityPower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case PositiveEffect.Focus:
                await ApplyPower<FocusPower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case PositiveEffect.Buffer:
                await ApplyPower<BufferPower>(choiceContext, cardPlay, 1m);
                break;
            case PositiveEffect.EnergyNextTurn:
                await ApplyPower<EnergyNextTurnPower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case PositiveEffect.DrawNextTurn:
                await ApplyPower<DrawCardsNextTurnPower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case PositiveEffect.Barricade:
                await ApplyPower<BarricadePower>(choiceContext, cardPlay, 1m);
                break;
            case PositiveEffect.BlockNextTurn:
                await ApplyPower<BlockNextTurnPower>(choiceContext, cardPlay, RollInclusive(5, 10));
                break;
            case PositiveEffect.Clarity:
                await ApplyPower<ClarityPower>(choiceContext, cardPlay, 3m);
                break;
            case PositiveEffect.Intangible:
                await ApplyPower<IntangiblePower>(choiceContext, cardPlay, 1m);
                break;
            case PositiveEffect.Thorns:
                await ApplyPower<ThornsPower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case PositiveEffect.Vigor:
                await ApplyPower<VigorPower>(choiceContext, cardPlay, RollInclusive(3, 6));
                break;
            case PositiveEffect.FreeAttack:
                await ApplyPower<FreeAttackPower>(choiceContext, cardPlay, 1m);
                break;
            case PositiveEffect.FreeSkill:
                await ApplyPower<FreeSkillPower>(choiceContext, cardPlay, 1m);
                break;
            case PositiveEffect.FreePower:
                await ApplyPower<FreePowerPower>(choiceContext, cardPlay, 1m);
                break;
            case PositiveEffect.MaxHp:
                await CreatureCmd.GainMaxHp(Owner.Creature, 3m);
                break;
            case PositiveEffect.Coolheaded:
                await AddGeneratedCardToHand<Coolheaded>();
                break;
            case PositiveEffect.Scrawl:
                await AddGeneratedCardToHand<Scrawl>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
        }
    }

    private async Task ApplyNegativeEffect(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        NegativeEffect effect)
    {
        switch (effect)
        {
            case NegativeEffect.LoseStrength:
                await ApplyPower<StrengthPower>(choiceContext, cardPlay, -1m);
                break;
            case NegativeEffect.LoseDexterity:
                await ApplyPower<DexterityPower>(choiceContext, cardPlay, -1m);
                break;
            case NegativeEffect.LoseFocus:
                await ApplyPower<FocusPower>(choiceContext, cardPlay, -1m);
                break;
            case NegativeEffect.Weak:
                await ApplyPower<WeakPower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case NegativeEffect.Frail:
                await ApplyPower<FrailPower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case NegativeEffect.Vulnerable:
                await ApplyPower<VulnerablePower>(choiceContext, cardPlay, RollInclusive(1, 2));
                break;
            case NegativeEffect.NoBlock:
                await ApplyPower<NoBlockPower>(choiceContext, cardPlay, 1m);
                break;
            case NegativeEffect.NoDraw:
                await ApplyPower<NoDrawPower>(choiceContext, cardPlay, 1m);
                break;
            case NegativeEffect.NoEnergyGain:
                await ApplyPower<NoEnergyGainPower>(choiceContext, cardPlay, 1m);
                break;
            case NegativeEffect.Confused:
                await ApplyPower<ConfusedPower>(choiceContext, cardPlay, 1m);
                break;
            case NegativeEffect.Slimed:
                await AddGeneratedCardsToHand<Slimed>(2);
                break;
            case NegativeEffect.Burn:
                await AddGeneratedCardToHand<Burn>();
                break;
            case NegativeEffect.Wound:
                await AddGeneratedCardToHand<Wound>();
                break;
            case NegativeEffect.Dazed:
                await AddGeneratedCardsToDiscard<Dazed>(RollInclusive(2, 3));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(effect), effect, null);
        }
    }

    private async Task ApplyPower<TPower>(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay,
        decimal amount)
        where TPower : PowerModel, new()
    {
        await PowerCmd.Apply<TPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
    }

    private int RollInclusive(int minimum, int maximum) =>
        minimum + Owner.RunState.Rng.CombatCardSelection.NextInt(maximum - minimum + 1);

    private async Task AddGeneratedCardToHand<TCard>()
        where TCard : CardModel
    {
        TCard card = CombatState!.CreateCard<TCard>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }

    private async Task AddGeneratedCardsToHand<TCard>(int count)
        where TCard : CardModel
    {
        for (int index = 0; index < count; index++)
        {
            await AddGeneratedCardToHand<TCard>();
        }
    }

    private async Task AddGeneratedCardsToDiscard<TCard>(int count)
        where TCard : CardModel
    {
        for (int index = 0; index < count; index++)
        {
            TCard card = CombatState!.CreateCard<TCard>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, Owner);
        }
    }
}
