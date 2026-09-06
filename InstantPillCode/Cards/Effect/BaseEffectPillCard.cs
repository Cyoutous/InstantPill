using System.Collections.Generic;
using System.Threading.Tasks;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace InstantPill.InstantPillCode.Cards.Effect;

/// <summary>
/// Shared structural model for identified effect pills.
/// Individual cards will add their gameplay behaviour when their effects are implemented.
/// </summary>
public abstract class BaseEffectPillCard : BasePillCard
{
    /// <summary>
    /// Identified effect pills use the combat consume visual in place of the normal Power-card
    /// flight. Mystery pills deliberately do not inherit this marker, so they retain the base
    /// Power presentation until they reveal into an effect pill.
    /// </summary>
    public virtual bool UsesEffectPillConsumeVfx => true;

    /// <summary>
    /// Internal balance metadata used only when constructing a run's candidate-effect pool.
    /// It intentionally has no localization, keyword, or card-facing representation.
    /// </summary>
    public enum EffectPillGrade
    {
        /// <summary>
        /// Developer-only grade. The card remains registered and can be created directly, but is excluded
        /// from a newly initialized run's candidate-effect pool.
        /// </summary>
        Excluded = -1,
        Grade0 = 0,
        Grade1 = 1,
        Grade2 = 2,
        Grade3 = 3
    }

    protected BaseEffectPillCard()
        : base(0)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        InstantPillKeywords.Capsule
    ];

    public override string PortraitPath => PillPortraitCatalog.GetEffectPortraitPath(this);

    // Revealing an effect changes card text and behaviour, not the identity image shown to player.
    public override string BetaPortraitPath => PortraitPath;

    public override IEnumerable<string> AllPortraitPaths => PillPortraitCatalog.AllEffectPortraitPaths;

    /// <summary>
    /// Hidden generation weight class. Individual effect pills can override this when the pool
    /// composition algorithm begins using grades.
    /// </summary>
    public virtual EffectPillGrade Grade => EffectPillGrade.Grade0;

    protected abstract int StrengthAmount { get; }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            StrengthAmount,
            Owner.Creature,
            this);
    }
}
