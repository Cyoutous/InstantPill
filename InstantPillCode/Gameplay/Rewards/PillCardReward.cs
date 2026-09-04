using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>
/// A normal card-selection reward whose choices are explicitly supplied by the capsule pool.
/// All interaction, history, and deck-add behavior remain the game's CardReward implementation.
/// </summary>
internal sealed class PillCardReward : CardReward
{
    public PillCardReward(
        IEnumerable<CardModel> cardsToOffer,
        Player player,
        CardCreationOptions rerollOptions,
        PlayerChoiceSynchronizer? synchronizer = null)
        : base(cardsToOffer, CardCreationSource.Other, player, rerollOptions, synchronizer)
    {
        CanReroll = false;
    }

    public override LocString Description => new("gameplay_ui", "INSTANTPILL-REWARD_ADD_PILL");
}
