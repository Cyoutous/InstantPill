using System;
using System.Collections.Generic;
using InstantPill.InstantPillCode.Configuration;
using InstantPill.InstantPillCode.Gameplay.Rewards;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace InstantPill.InstantPillCode.Gameplay.PHD;

/// <summary>Supplies False PHD's configured rarity while a new run builds relic grab bags.</summary>
public static class FalsePhdRelicRarityService
{
    private static readonly Stack<RelicRarity> SetupOverrides = new();

    public static RelicRarity CurrentRarity => SetupOverrides.Count > 0
        ? SetupOverrides.Peek()
        : PhdRelicRarityRules.ToRelicRarity(
            PillRewardSettings.GetValue(PillRewardSettings.FalsePhdRelicRarityKey) as string);

    public static IDisposable PushNewRunRules(PillRewardRulesSnapshot rules)
    {
        SetupOverrides.Push(PhdRelicRarityRules.ToRelicRarity(rules.FalsePhdRelicRarity));
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
