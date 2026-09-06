using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Cards;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace InstantPill.InstantPillCode.Cards.Generated;

/// <summary>
/// A generated, purging heart that heals a percentage of its owner's maximum HP.
/// It is intentionally excluded from normal card pools by <see cref="BaseGeneratedCard"/>.
/// </summary>
[CustomID("INSTANTPILL-HEART")]
public sealed class Heart : BaseGeneratedCard
{
    public override string PortraitPath =>
    "res://InstantPill/images/cards/generated/heart.png";

    private const decimal PercentDivisor = 100m;

    public Heart()
        : base(0, CardType.Skill, TargetType.Self)
    {
    }

    public override int MaxUpgradeLevel => 1;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        BaseLibKeywords.Purge
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new RepeatVar(8)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal healAmount = Owner.Creature.MaxHp * DynamicVars.Repeat.BaseValue / PercentDivisor;
        await CreatureCmd.Heal(Owner.Creature, healAmount);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(2);
    }
}
