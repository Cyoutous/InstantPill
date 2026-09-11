using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using InstantPill.InstantPillCode.Gameplay.PHD;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace InstantPill.InstantPillCode.Relics;

/// <summary>
/// False PHD's persistent potency counter and configured relic-pool placement.
/// </summary>
[Pool(typeof(SharedRelicPool))]
[CustomID(RelicId)]
public sealed class FalsePhdRelic : CustomRelicModel
{
    public const string RelicId = "INSTANTPILL-FALSE_PHD";

    public override string PackedIconPath => "res://InstantPill/images/relics/false_phd.png";

    protected override string PackedIconOutlinePath => "res://InstantPill/images/relics/false_phd_outline.png";

    protected override string BigIconPath => "res://InstantPill/images/relics/big/false_phd.png";

    public override RelicRarity Rarity => FalsePhdRelicRarityService.CurrentRarity;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Potency;

    public override async Task AfterObtained()
    {
        // The pool assignment remains immutable, while RollAssignedEffectCardIdForPlayer
        // materializes the selected effect for this owner (including False PHD conversion).
        string capsuleId = PillPoolService.RollAssignedEffectCardIdForPlayer(Owner);
        CardModel canonicalCapsule = ModelDb.GetById<CardModel>(
            new ModelId(ModelId.SlugifyCategory<CardModel>(), capsuleId));
        CardModel capsule = Owner.RunState.CreateCard(canonicalCapsule, Owner);
        CardPileAddResult addition = await CardPileCmd.Add(capsule, PileType.Deck);
        CardCmd.PreviewCardPileAdd([addition], 2f);

        // Like PHD, False PHD immediately identifies every still-unknown capsule. The shared
        // resolver decides whether those identities materialize as False PHD replacements,
        // normal effects (when both relics are held), or unchanged effects.
        await PhdPillConversionService.OnFalsePhdObtained(Owner);
    }

    [SavedProperty]
    public int Potency
    {
        get => _potency;
        private set
        {
            AssertMutable();
            _potency = value;
            InvokeDisplayAmountChanged();
        }
    }

    public void AddPotency(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        Potency = checked(Potency + amount);
    }

    public override async Task BeforeCombatStart()
    {
        if (Potency <= 0)
        {
            return;
        }

        Flash();
        decimal amount = Potency;
        ThrowingPlayerChoiceContext context = new();
        await PowerCmd.Apply<StrengthPower>(context, Owner.Creature, amount, Owner.Creature, null);
        await PowerCmd.Apply<DexterityPower>(context, Owner.Creature, amount, Owner.Creature, null);
        await PowerCmd.Apply<FocusPower>(context, Owner.Creature, amount, Owner.Creature, null);
    }

    private int _potency;
}
