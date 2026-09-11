using System;
using System.Collections.Generic;
using InstantPill.InstantPillCode.Configuration;
using InstantPill.InstantPillCode.Gameplay.Rewards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace InstantPill.InstantPillCode.Gameplay.PHD;

/// <summary>
/// Supplies PHD's rarity while a new run builds its relic grab bags. The game's grab bags hold
/// canonical relic models, which have no owner/run reference, so the short-lived setup scope is
/// the narrow point where a per-run multiplayer-host setting can be applied safely.
/// </summary>
public static class PhdRelicRarityService
{
    private static readonly Stack<RelicRarity> SetupOverrides = new();

    public static RelicRarity CurrentRarity => SetupOverrides.Count > 0
        ? SetupOverrides.Peek()
        : PhdRelicRarityRules.ToRelicRarity(
            PillRewardSettings.GetValue(PillRewardSettings.PhdRelicRarityKey) as string);

    public static IDisposable PushNewRunRules(PillRewardRulesSnapshot rules)
    {
        SetupOverrides.Push(PhdRelicRarityRules.ToRelicRarity(rules.PhdRelicRarity));
        return new PopScope();
    }

    private sealed class PopScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            if (SetupOverrides.Count > 0)
            {
                SetupOverrides.Pop();
            }
        }
    }
}
