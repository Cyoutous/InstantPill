using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Cards.Generated;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// A grade-2 pill that takes up to 12% maximum-HP loss without being lethal,
/// then permanently adds a random one to four base Hearts to the owner's deck.
/// </summary>
[CustomID("INSTANTPILL-HEMATEMESIS")]
public sealed class Hematemesis : BaseEffectPillCard
{
    private const string SoundPath = "res://audio/hematemisis 1.wav";
    private const float SoundVolume = 1f;
    private const decimal HpLossPercent = 0.12m;
    private const int MinHeartCount = 1;
    private const int MaxHeartCount = 4;

    protected override int StrengthAmount => 0;

    public override EffectPillGrade Grade => EffectPillGrade.Grade2;

    public override string? FalsePhdReplacementCardId => "INSTANTPILL-FPHD_BAD_TRIP";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Heart>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (LocalContext.IsMine(this))
        {
            PillAudio.PlayOneShot(SoundPath, SoundVolume);
        }

        decimal maxNonLethalLoss = Math.Max(0, Owner.Creature.CurrentHp - 1);
        decimal hpLoss = Math.Min(Owner.Creature.MaxHp * HpLossPercent, maxNonLethalLoss);
        if (hpLoss > 0m)
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner.Creature,
                hpLoss,
                ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move,
                this,
                cardPlay);
        }

        int heartCount = MinHeartCount + Owner.RunState.Rng.CombatCardSelection.NextInt(
            MaxHeartCount - MinHeartCount + 1);
        var addedHearts = new List<CardPileAddResult>(heartCount);
        for (int index = 0; index < heartCount; index++)
        {
            Heart heart = Owner.RunState.CreateCard<Heart>(Owner);
            addedHearts.Add(await CardPileCmd.Add(heart, PileType.Deck));
        }

        CardCmd.PreviewCardPileAdd(addedHearts, 2f);
    }
}
