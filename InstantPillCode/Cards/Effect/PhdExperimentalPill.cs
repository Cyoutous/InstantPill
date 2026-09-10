using System;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// PHD-only Experimental Pill. It uses the original uniformly weighted positive-effect table
/// and its original numeric ranges, but deliberately omits Experimental Pill's negative roll.
/// </summary>
[CustomID("INSTANTPILL-PHD_EXPERIMENTAL_PILL")]
public sealed class PhdExperimentalPill : BaseEffectPillCard
{
    private enum PositiveEffect
    {
        Strength, Dexterity, Focus, Buffer, EnergyNextTurn, DrawNextTurn, Barricade,
        BlockNextTurn, Clarity, Intangible, Thorns, Vigor, FreeAttack, FreeSkill,
        FreePower, MaxHp, Coolheaded, Scrawl
    }

    public PhdExperimentalPill() : base(showInCardLibrary: false) { }

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Excluded;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        PositiveEffect positive = (PositiveEffect)Owner.RunState.Rng.CombatCardSelection
            .NextInt(Enum.GetValues<PositiveEffect>().Length);
        await ApplyPositiveEffect(choiceContext, cardPlay, positive);
    }

    private async Task ApplyPositiveEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay, PositiveEffect effect)
    {
        switch (effect)
        {
            case PositiveEffect.Strength: await ApplyPower<StrengthPower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case PositiveEffect.Dexterity: await ApplyPower<DexterityPower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case PositiveEffect.Focus: await ApplyPower<FocusPower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case PositiveEffect.Buffer: await ApplyPower<BufferPower>(choiceContext, cardPlay, 1m); break;
            case PositiveEffect.EnergyNextTurn: await ApplyPower<EnergyNextTurnPower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case PositiveEffect.DrawNextTurn: await ApplyPower<DrawCardsNextTurnPower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case PositiveEffect.Barricade: await ApplyPower<BarricadePower>(choiceContext, cardPlay, 1m); break;
            case PositiveEffect.BlockNextTurn: await ApplyPower<BlockNextTurnPower>(choiceContext, cardPlay, RollInclusive(5, 10)); break;
            case PositiveEffect.Clarity: await ApplyPower<ClarityPower>(choiceContext, cardPlay, 3m); break;
            case PositiveEffect.Intangible: await ApplyPower<IntangiblePower>(choiceContext, cardPlay, 1m); break;
            case PositiveEffect.Thorns: await ApplyPower<ThornsPower>(choiceContext, cardPlay, RollInclusive(1, 2)); break;
            case PositiveEffect.Vigor: await ApplyPower<VigorPower>(choiceContext, cardPlay, RollInclusive(3, 6)); break;
            case PositiveEffect.FreeAttack: await ApplyPower<FreeAttackPower>(choiceContext, cardPlay, 1m); break;
            case PositiveEffect.FreeSkill: await ApplyPower<FreeSkillPower>(choiceContext, cardPlay, 1m); break;
            case PositiveEffect.FreePower: await ApplyPower<FreePowerPower>(choiceContext, cardPlay, 1m); break;
            case PositiveEffect.MaxHp: await CreatureCmd.GainMaxHp(Owner.Creature, 3m); break;
            case PositiveEffect.Coolheaded: await AddGeneratedCardToHand<Coolheaded>(); break;
            case PositiveEffect.Scrawl: await AddGeneratedCardToHand<Scrawl>(); break;
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
}
