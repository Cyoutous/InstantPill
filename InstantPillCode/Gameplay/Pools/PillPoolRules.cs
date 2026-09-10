using System;
using System.Collections.Generic;
using InstantPill.InstantPillCode.Cards.Effect;
using InstantPill.InstantPillCode.Gameplay.PHD;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// Central rules for the per-player pill pools.
/// </summary>
public static class PillPoolRules
{
    public const int SchemaVersion = 2;

    // There are currently 13 mystery pill models. Each active mystery slot receives
    // exactly one distinct candidate effect over the course of a run.
    public const int PoolSize = 13;

    // Guaranteed high-grade draws happen in this order. Any unfilled quota is deliberately
    // released into the final all-grade fill instead of being reassigned to another grade.
    public const int Grade3Quota = 3;
    public const int Grade2Quota = 4;
    public const int Grade1Quota = 2;
    public const int Grade0Quota = 1;

    internal static IReadOnlyList<(BaseEffectPillCard.EffectPillGrade Grade, int Count)> PriorityGradeQuotas { get; } =
    [
        (BaseEffectPillCard.EffectPillGrade.Grade3, Grade3Quota),
        (BaseEffectPillCard.EffectPillGrade.Grade2, Grade2Quota),
        (BaseEffectPillCard.EffectPillGrade.Grade1, Grade1Quota),
        (BaseEffectPillCard.EffectPillGrade.Grade0, Grade0Quota)
    ];

    internal static void ValidateCatalog()
    {
        if (PoolSize < 1)
        {
            throw new InvalidOperationException("InstantPill pool size must be at least one.");
        }

        if (PoolSize > PillPoolCatalog.MysteryPillIds.Count)
        {
            throw new InvalidOperationException("InstantPill pool size exceeds the available mystery pill models.");
        }

        PillPhdResolver.ValidateReplacementMetadata(PillPoolCatalog.EffectPillIds);
    }
}
