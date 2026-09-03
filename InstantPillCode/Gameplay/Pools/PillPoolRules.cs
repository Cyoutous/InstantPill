using System;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// Central rules for the per-player pill pools.
/// </summary>
public static class PillPoolRules
{
    public const int SchemaVersion = 1;

    // There are currently 20 mystery pill models. Each active mystery slot receives
    // exactly one distinct candidate effect over the course of a run.
    public const int PoolSize = 20;

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

        if (PoolSize > PillPoolCatalog.EffectPillIds.Count)
        {
            throw new InvalidOperationException("InstantPill pool size exceeds the available effect pill models.");
        }
    }
}
