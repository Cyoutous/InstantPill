using System;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>
/// The only source of hard-coded balance defaults. Player-editable settings begin from these
/// values and are persisted outside the mod package by <c>PillRewardSettings</c>.
/// </summary>
public static class PillRewardDefaults
{
    public static PillRewardRulesSnapshot Create() => new()
    {
        IndependentDrop = new PillOddsRules
        {
            BaseOdds = 0.45f,
            SuccessDecrease = 0.25f,
            FailureIncrease = 0.35f,
            EliteBonus = 0.15f,
            BossBonus = 0.25f,
            MinimumOdds = 0f,
            MaximumOdds = 1f
        },
        PotionReplacement = new PillOddsRules
        {
            BaseOdds = 0.05f,
            SuccessDecrease = 0.025f,
            FailureIncrease = 0.025f,
            EliteBonus = 0.05f,
            BossBonus = 0.025f,
            MinimumOdds = 0f,
            MaximumOdds = 1f
        },
        NormalChoiceCount = 3,
        EliteChoiceCount = 3,
        BossChoiceCount = 3,
        EnableNormalRewards = true,
        EnableEliteRewards = true,
        EnableBossRewards = true,
        PreventDuplicateCapsuleOptionsWithinCombat = false
    };
}

/// <summary>One mutable pity-odds channel, persisted as part of a run snapshot.</summary>
public sealed class PillOddsRules
{
    public float BaseOdds { get; set; }

    public float SuccessDecrease { get; set; }

    public float FailureIncrease { get; set; }

    public float EliteBonus { get; set; }

    public float BossBonus { get; set; }

    public float MinimumOdds { get; set; }

    public float MaximumOdds { get; set; }

    public PillOddsRules Clone() => new()
    {
        BaseOdds = BaseOdds,
        SuccessDecrease = SuccessDecrease,
        FailureIncrease = FailureIncrease,
        EliteBonus = EliteBonus,
        BossBonus = BossBonus,
        MinimumOdds = MinimumOdds,
        MaximumOdds = MaximumOdds
    };

    public void Normalize()
    {
        MinimumOdds = Math.Clamp(MinimumOdds, 0f, 1f);
        MaximumOdds = Math.Clamp(MaximumOdds, MinimumOdds, 1f);
        BaseOdds = Math.Clamp(BaseOdds, MinimumOdds, MaximumOdds);
        SuccessDecrease = Math.Clamp(SuccessDecrease, 0f, 1f);
        FailureIncrease = Math.Clamp(FailureIncrease, 0f, 1f);
        EliteBonus = Math.Clamp(EliteBonus, -1f, 1f);
        BossBonus = Math.Clamp(BossBonus, -1f, 1f);
    }
}

/// <summary>Complete, immutable-for-a-run reward configuration snapshot.</summary>
public sealed class PillRewardRulesSnapshot
{
    public PillOddsRules IndependentDrop { get; set; } = new();

    public PillOddsRules PotionReplacement { get; set; } = new();

    public int NormalChoiceCount { get; set; }

    public int EliteChoiceCount { get; set; }

    public int BossChoiceCount { get; set; }

    public bool EnableNormalRewards { get; set; }

    public bool EnableEliteRewards { get; set; }

    public bool EnableBossRewards { get; set; }

    public bool PreventDuplicateCapsuleOptionsWithinCombat { get; set; }

    public PillRewardRulesSnapshot Clone() => new()
    {
        IndependentDrop = IndependentDrop.Clone(),
        PotionReplacement = PotionReplacement.Clone(),
        NormalChoiceCount = NormalChoiceCount,
        EliteChoiceCount = EliteChoiceCount,
        BossChoiceCount = BossChoiceCount,
        EnableNormalRewards = EnableNormalRewards,
        EnableEliteRewards = EnableEliteRewards,
        EnableBossRewards = EnableBossRewards,
        PreventDuplicateCapsuleOptionsWithinCombat = PreventDuplicateCapsuleOptionsWithinCombat
    };

    public void Normalize(int maximumChoices)
    {
        IndependentDrop.Normalize();
        PotionReplacement.Normalize();
        NormalChoiceCount = Math.Clamp(NormalChoiceCount, 1, maximumChoices);
        EliteChoiceCount = Math.Clamp(EliteChoiceCount, 1, maximumChoices);
        BossChoiceCount = Math.Clamp(BossChoiceCount, 1, maximumChoices);
    }
}
