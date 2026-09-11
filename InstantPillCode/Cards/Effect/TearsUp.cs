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
/// A grade-3 pill that permanently adds a Negative-enchanted Feeding Frenzy to the owner's deck.
/// The enchantment is intentionally applied before the card enters the deck to test the resulting add animation.
/// </summary>
[CustomID("INSTANTPILL-TEARS_UP")]
public sealed class TearsUp : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/tears up 2.wav";
    private const float SoundVolume = 1f;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade3;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-TEARS_DOWN";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. HoverTipFactory.FromEnchantment<Negative>(),
        HoverTipFactory.FromCard<FeedingFrenzy>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        FeedingFrenzy feedingFrenzy = Owner.RunState.CreateCard<FeedingFrenzy>(Owner);

        // Deliberately apply before entering the deck.  This is the animation-compatibility experiment requested
        // for Tears Up; later cards can retain the usual native add-then-enchant ordering if preferred.
        NegativeEnchantmentService.TryApply(feedingFrenzy);

        CardPileAddResult result = await CardPileCmd.Add(feedingFrenzy, PileType.Deck);
        CardCmd.PreviewCardPileAdd([result], 2f);
    }
}
