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
/// A grade-3 pill that permanently adds a Negative-enchanted Doubt to the owner's deck.
/// The enchantment is intentionally applied before the card enters the deck to test the resulting add animation.
/// </summary>
[CustomID("INSTANTPILL-TEARS_DOWN")]
public sealed class TearsDown : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/tears down 5.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    public override string? PhdReplacementCardId => "INSTANTPILL-TEARS_UP";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. HoverTipFactory.FromEnchantment<Negative>(),
        HoverTipFactory.FromCard<Doubt>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        Doubt doubt = Owner.RunState.CreateCard<Doubt>(Owner);

        // Deliberately apply before entering the deck.  This is the animation-compatibility experiment requested
        // for Tears Down; later cards can retain the usual native add-then-enchant ordering if preferred.
        NegativeEnchantmentService.TryApply(doubt);

        CardPileAddResult result = await CardPileCmd.Add(doubt, PileType.Deck);
        CardCmd.PreviewCardPileAdd([result], 2f);
    }
}
