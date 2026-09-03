using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace InstantPill.InstantPillCode.Cards;

/// <summary>
/// Shared structural base for InstantPill cards.
/// Individual pill cards opt into their own keywords and play behaviour.
/// </summary>
[Pool(typeof(TokenCardPool))]
public abstract class BasePillCard : CustomCardModel
{
    // Uses the same mechanism as unupgradable vanilla cards such as Dazed.
    // All standard smithing and CardCmd upgrade paths therefore skip every pill card.
    public override int MaxUpgradeLevel => 0;

    protected BasePillCard(int energyCost, bool showInCardLibrary = true)
        : base(energyCost, CardType.Power, CardRarity.Token, TargetType.None, showInCardLibrary)
    {
    }
}
