using System;
using InstantPill.InstantPillCode.Configuration;

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
            BaseOdds = 0.75f,
            SuccessDecrease = 0.15f,
            FailureIncrease = 0.35f,
            EliteBonus = 0.15f,
            BossBonus = 0.25f,
            MinimumOdds = 0f,
            MaximumOdds = 1f
        },
        PotionReplacement = new PillOddsRules
        {
            BaseOdds = 0.05f,
            SuccessDecrease = 0.2f,
            FailureIncrease = 0.15f,
            EliteBonus = 0.1f,
            BossBonus = 0.2f,
            MinimumOdds = 0f,
            MaximumOdds = 1f
        },
        NormalChoiceCount = 2,
        EliteChoiceCount = 3,
        BossChoiceCount = 4,
        NormalGenerationAttempts = 1,
        EliteGenerationAttempts = 2,
        BossGenerationAttempts = 3,
        NormalGuaranteedRewardCount = 0,
        EliteGuaranteedRewardCount = 1,
        BossGuaranteedRewardCount = 2,
        InitialMysteryPillCount = 1,
        PhdRelicRarity = PhdRelicRarityRules.DefaultValue,
        FalsePhdRelicRarity = PhdRelicRarityRules.DefaultValue,
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

    /// <summary>Independent capsule reward probability rolls performed in each room type.</summary>
    public int NormalGenerationAttempts { get; set; }

    public int EliteGenerationAttempts { get; set; }

    public int BossGenerationAttempts { get; set; }

    /// <summary>Minimum independent capsule reward panels granted in each room type.</summary>
    public int NormalGuaranteedRewardCount { get; set; }

    public int EliteGuaranteedRewardCount { get; set; }

    public int BossGuaranteedRewardCount { get; set; }

    /// <summary>How many mystery capsules each player receives directly in their starting deck.</summary>
    public int InitialMysteryPillCount { get; set; }

    /// <summary>Configured PHD pool; frozen at run creation alongside the other gameplay rules.</summary>
    public string PhdRelicRarity { get; set; } = PhdRelicRarityRules.DefaultValue;

    /// <summary>Configured False PHD pool; frozen at run creation alongside the other gameplay rules.</summary>
    public string FalsePhdRelicRarity { get; set; } = PhdRelicRarityRules.DefaultValue;

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
        NormalGenerationAttempts = NormalGenerationAttempts,
        EliteGenerationAttempts = EliteGenerationAttempts,
        BossGenerationAttempts = BossGenerationAttempts,
        NormalGuaranteedRewardCount = NormalGuaranteedRewardCount,
        EliteGuaranteedRewardCount = EliteGuaranteedRewardCount,
        BossGuaranteedRewardCount = BossGuaranteedRewardCount,
        InitialMysteryPillCount = InitialMysteryPillCount,
        PhdRelicRarity = PhdRelicRarity,
        FalsePhdRelicRarity = FalsePhdRelicRarity,
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
        NormalGenerationAttempts = Math.Clamp(NormalGenerationAttempts, 0, maximumChoices);
        EliteGenerationAttempts = Math.Clamp(EliteGenerationAttempts, 0, maximumChoices);
        BossGenerationAttempts = Math.Clamp(BossGenerationAttempts, 0, maximumChoices);
        NormalGuaranteedRewardCount = Math.Clamp(NormalGuaranteedRewardCount, 0, maximumChoices);
        EliteGuaranteedRewardCount = Math.Clamp(EliteGuaranteedRewardCount, 0, maximumChoices);
        BossGuaranteedRewardCount = Math.Clamp(BossGuaranteedRewardCount, 0, maximumChoices);
        InitialMysteryPillCount = Math.Clamp(InitialMysteryPillCount, 0, 1000);
        PhdRelicRarity = PhdRelicRarityRules.Normalize(PhdRelicRarity);
        FalsePhdRelicRarity = PhdRelicRarityRules.Normalize(FalsePhdRelicRarity);
    }
}
