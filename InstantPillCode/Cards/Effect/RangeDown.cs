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
using VoidCard = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-3 pill that permanently adds a Negative-enchanted original Void to the owner's deck.
/// The enchantment is applied before the native pile-add command so its card-add preview uses the
/// completed enchanted card model.
/// </summary>
[CustomID("INSTANTPILL-RANGE_DOWN")]
public sealed class RangeDown : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/range down 2.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    public override string? PhdReplacementCardId => "INSTANTPILL-RANGE_UP";

    public override bool IsExtremelyPowerfulEffect => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. HoverTipFactory.FromEnchantment<Negative>(),
        HoverTipFactory.FromCard<VoidCard>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        VoidCard voidCard = Owner.RunState.CreateCard<VoidCard>(Owner);
        NegativeEnchantmentService.TryApply(voidCard);

        CardPileAddResult result = await CardPileCmd.Add(voidCard, PileType.Deck);
        CardCmd.PreviewCardPileAdd([result], 2f);
    }
}
