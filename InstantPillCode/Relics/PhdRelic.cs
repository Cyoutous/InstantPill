using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Cards.Generated;
using InstantPill.InstantPillCode.Gameplay.PHD;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace InstantPill.InstantPillCode.Relics;

/// <summary>
/// PHD is a shop relic. Its pickup callback reveals and materializes the owner's pills.
/// </summary>
[Pool(typeof(SharedRelicPool))]
[CustomID(RelicId)]
public sealed class PhdRelic : CustomRelicModel
{
    public const string RelicId = "INSTANTPILL-PHD";
    private const int HeartCount = 2;

    public override string PackedIconPath => "res://InstantPill/images/relics/phd.png";

    protected override string PackedIconOutlinePath => "res://InstantPill/images/relics/phd_outline.png";

    protected override string BigIconPath => "res://InstantPill/images/relics/big/phd.png";

    public override RelicRarity Rarity => RelicRarity.Shop;

    public override async Task AfterObtained()
    {
        var additions = new List<CardPileAddResult>(HeartCount + 1);
        for (int index = 0; index < HeartCount; index++)
        {
            Heart heart = Owner.RunState.CreateCard<Heart>(Owner);
            additions.Add(await CardPileCmd.Add(heart, PileType.Deck));
        }

        string capsuleId = PillPoolService.RollAssignedEffectCardIdForPlayer(Owner);
        CardModel canonicalCapsule = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), capsuleId));
        CardModel capsule = Owner.RunState.CreateCard(canonicalCapsule, Owner);
        additions.Add(await CardPileCmd.Add(capsule, PileType.Deck));
        CardCmd.PreviewCardPileAdd(additions, 2f);

        await PhdPillConversionService.OnPhdObtained(Owner);
    }
}
