using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Enchantments;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-3 pill that permanently adds a Negative-enchanted original Production to the owner's deck.
/// </summary>
[CustomID("INSTANTPILL-RANGE_UP")]
public sealed class RangeUp : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/range up 1.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-RANGE_DOWN";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. HoverTipFactory.FromEnchantment<Negative>(),
        HoverTipFactory.FromCard<Production>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        Production production = Owner.RunState.CreateCard<Production>(Owner);
        NegativeEnchantmentService.TryApply(production);

        CardPileAddResult result = await CardPileCmd.Add(production, PileType.Deck);
        CardCmd.PreviewCardPileAdd([result], 2f);
    }
}
