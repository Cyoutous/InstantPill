using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Gameplay.BlueFly;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace InstantPill.InstantPillCode.Cards.Generated;

/// <summary>
/// A generated attack card. A manually played copy begins one dynamic chain across every Blue Fly
/// currently in hand, including flies drawn while that chain resolves.
/// </summary>
[CustomID("INSTANTPILL-BLUE_FLY")]
public sealed class BlueFly : BaseGeneratedCard
{
    private const int CardsToDraw = 1;

    public override string PortraitPath =>
    "res://InstantPill/images/cards/generated/blue_fly.png";

    /// <summary>
    /// Blue Fly is the sole generated card that can be upgraded. All other generated cards remain
    /// non-upgradeable through <see cref="BaseGeneratedCard"/>.
    /// </summary>
    public override int MaxUpgradeLevel => 1;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3m, ValueProp.Move)
    ];

    public BlueFly()
        : base(0, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext, CardsToDraw, Owner);

        if (!cardPlay.IsAutoPlay)
        {
            await BlueFlyChainService.PlayHandFlies(choiceContext, this, cardPlay.Target);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
