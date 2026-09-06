using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Cards.Generated;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-2 pill that turns transformable Status cards in the active combat piles into Blue Flies.
/// Frantic Escape is deliberately excluded by model identity because it carries encounter-specific state.
/// </summary>
[CustomID("INSTANTPILL-INFESTED_1")]
public sealed class Infested1 : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/infested!_02.wav";
    private const float SoundVolume = 1f;
    private const string FranticEscapeId = "FRANTIC_ESCAPE";

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(InstantPillKeywords.StatusCard),
        HoverTipFactory.FromCard<BlueFly>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        CardModel[] combatCards =
        [
            .. Owner.PlayerCombatState!.Hand.Cards,
            .. Owner.PlayerCombatState.DrawPile.Cards,
            .. Owner.PlayerCombatState.DiscardPile.Cards
        ];
        CardModel[] statusCards = combatCards.Where(card =>
            card.Type == CardType.Status
            && card.Id.Entry != FranticEscapeId
            && card.IsTransformable)
        .ToArray();

        List<CardTransformation> transformations = statusCards
            .Select(card => new CardTransformation(
                card,
                CombatState!.CreateCard<BlueFly>(Owner)))
            .ToList();
        if (transformations.Count > 0)
        {
            await CardCmd.Transform(transformations, null);
        }
    }
}
