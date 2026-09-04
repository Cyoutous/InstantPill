using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Cards;
using InstantPill.InstantPillCode.Content;
using InstantPill.InstantPillCode.Gameplay.Pools;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace InstantPill.InstantPillCode.Cards.Mystery;

/// <summary>
/// Shared no-effect card model for unidentified pills.
/// The later reveal system will decide their behaviour when they are played.
/// </summary>
public abstract class BaseMysteryPillCard : BasePillCard
{
    protected BaseMysteryPillCard()
        : base(0)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        InstantPillKeywords.Capsule,
        InstantPillKeywords.Mystery,
        BaseLibKeywords.Purge
    ];

    public override string PortraitPath => PillPortraitCatalog.GetMysteryPortraitPath(Id.Entry);

    // One image is intentionally shared between normal and beta-art display modes.
    public override string BetaPortraitPath => PortraitPath;

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Mapped mystery pills are intercepted before this method and substituted by their effect
        // card. Reaching this branch therefore means this identity is outside the current pool.
        if (PillPoolService.TryGetAssignedEffectCardId(Owner, Id.Entry) == null && LocalContext.IsMine(this))
        {
            string text = new LocString("combat_messages", "INSTANTPILL-UNMAPPED_MYSTERY_PILL_THOUGHT")
                .GetFormattedText();
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(
                NThoughtBubbleVfx.Create(text, Owner.Creature, 1.5));
        }

        return Task.CompletedTask;
    }
}
