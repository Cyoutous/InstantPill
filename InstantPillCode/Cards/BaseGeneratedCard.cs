using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace InstantPill.InstantPillCode.Cards;

/// <summary>
/// Shared base for combat-generated cards. Token rarity keeps these cards out of ordinary reward
/// and shop pools; individual cards remain responsible for their own creation effects.
/// </summary>
[Pool(typeof(TokenCardPool))]
public abstract class BaseGeneratedCard : CustomCardModel
{
    protected BaseGeneratedCard(
        int energyCost,
        CardType type,
        TargetType targetType,
        bool showInCardLibrary = true)
        : base(energyCost, type, CardRarity.Token, targetType, showInCardLibrary)
    {
    }

    public override int MaxUpgradeLevel => 0;
}
