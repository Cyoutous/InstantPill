using System;
using System.Globalization;
using Godot;
using InstantPill.InstantPillCode.Gameplay.Rewards;

namespace InstantPill.InstantPillCode.Configuration;

/// <summary>
/// Global, player-editable reward settings. These values are read only when a new run creates
/// its <see cref="PillRewardRulesSnapshot"/>; an existing run keeps its saved rules snapshot.
/// </summary>
public static class PillRewardSettings
{
    private const string SaveDirectory = "user://InstantPill";
    private const string SavePath = SaveDirectory + "/reward_settings.cfg";
    private const string Section = "reward_rules";

    private static PillRewardSettingsData _data = CreateDefaultData();

    public static void Load()
    {
        _data = CreateDefaultData();
        ConfigFile config = new();
        if (config.Load(SavePath) != Error.Ok)
        {
            return;
        }

        foreach (string key in AllKeys)
        {
            if (!config.HasSectionKey(Section, key))
            {
                continue;
            }

            TrySet(key, config.GetValue(Section, key));
        }
    }

    public static void Save()
    {
        Normalize();

        Error directoryResult = DirAccess.MakeDirRecursiveAbsolute(SaveDirectory);
        if (directoryResult != Error.Ok)
        {
            MainFile.Logger.Warn($"InstantPill could not create the reward-settings directory ({directoryResult}).");
            return;
        }

        ConfigFile config = new();
        foreach (string key in AllKeys)
        {
            switch (GetValue(key))
            {
                case int integerValue:
                    config.SetValue(Section, key, integerValue);
                    break;
                case bool booleanValue:
                    config.SetValue(Section, key, booleanValue);
                    break;
            }
        }

        Error result = config.Save(SavePath);
        if (result != Error.Ok)
        {
            MainFile.Logger.Warn($"InstantPill could not save reward settings ({result}).");
        }
    }

    public static void ResetToDefaults() => _data = CreateDefaultData();

    public static object? GetValue(string key) => key switch
    {
        IndependentBaseOddsKey => _data.IndependentBaseOddsPercent,
        IndependentSuccessDecreaseKey => _data.IndependentSuccessDecreasePercent,
        IndependentFailureIncreaseKey => _data.IndependentFailureIncreasePercent,
        IndependentEliteBonusKey => _data.IndependentEliteBonusPercent,
        IndependentBossBonusKey => _data.IndependentBossBonusPercent,
        IndependentMinimumOddsKey => _data.IndependentMinimumOddsPercent,
        IndependentMaximumOddsKey => _data.IndependentMaximumOddsPercent,
        ReplacementBaseOddsKey => _data.ReplacementBaseOddsPercent,
        ReplacementSuccessDecreaseKey => _data.ReplacementSuccessDecreasePercent,
        ReplacementFailureIncreaseKey => _data.ReplacementFailureIncreasePercent,
        ReplacementEliteBonusKey => _data.ReplacementEliteBonusPercent,
        ReplacementBossBonusKey => _data.ReplacementBossBonusPercent,
        ReplacementMinimumOddsKey => _data.ReplacementMinimumOddsPercent,
        ReplacementMaximumOddsKey => _data.ReplacementMaximumOddsPercent,
        NormalChoiceCountKey => _data.NormalChoiceCount,
        EliteChoiceCountKey => _data.EliteChoiceCount,
        BossChoiceCountKey => _data.BossChoiceCount,
        NormalGenerationAttemptsKey => _data.NormalGenerationAttempts,
        EliteGenerationAttemptsKey => _data.EliteGenerationAttempts,
        BossGenerationAttemptsKey => _data.BossGenerationAttempts,
        NormalGuaranteedRewardCountKey => _data.NormalGuaranteedRewardCount,
        EliteGuaranteedRewardCountKey => _data.EliteGuaranteedRewardCount,
        BossGuaranteedRewardCountKey => _data.BossGuaranteedRewardCount,
        InitialMysteryPillCountKey => _data.InitialMysteryPillCount,
        SyncMultiplayerParametersWithHostKey => _data.SyncMultiplayerParametersWithHost,
        EnableSharedMultiplayerPillPoolKey => _data.EnableSharedMultiplayerPillPool,
        EnableNormalRewardsKey => _data.EnableNormalRewards,
        EnableEliteRewardsKey => _data.EnableEliteRewards,
        EnableBossRewardsKey => _data.EnableBossRewards,
        PreventDuplicateOptionsKey => _data.PreventDuplicateCapsuleOptionsWithinCombat,
        _ => null
    };

    /// <summary>Returns false for an unknown key or a value that cannot be converted safely.</summary>
    public static bool TrySet(string key, object? value)
    {
        if (IsBooleanKey(key))
        {
            if (!TryConvertBoolean(value, out bool booleanValue))
            {
                return false;
            }

            switch (key)
            {
                case EnableNormalRewardsKey: _data.EnableNormalRewards = booleanValue; break;
                case EnableEliteRewardsKey: _data.EnableEliteRewards = booleanValue; break;
                case EnableBossRewardsKey: _data.EnableBossRewards = booleanValue; break;
                case PreventDuplicateOptionsKey: _data.PreventDuplicateCapsuleOptionsWithinCombat = booleanValue; break;
                case EnableSharedMultiplayerPillPoolKey: _data.EnableSharedMultiplayerPillPool = booleanValue; break;
                case SyncMultiplayerParametersWithHostKey: _data.SyncMultiplayerParametersWithHost = booleanValue; break;
                default: return false;
            }

            return true;
        }

        if (!TryConvertInteger(value, out int integerValue))
        {
            return false;
        }

        switch (key)
        {
            case IndependentBaseOddsKey: _data.IndependentBaseOddsPercent = integerValue; break;
            case IndependentSuccessDecreaseKey: _data.IndependentSuccessDecreasePercent = integerValue; break;
            case IndependentFailureIncreaseKey: _data.IndependentFailureIncreasePercent = integerValue; break;
            case IndependentEliteBonusKey: _data.IndependentEliteBonusPercent = integerValue; break;
            case IndependentBossBonusKey: _data.IndependentBossBonusPercent = integerValue; break;
            case IndependentMinimumOddsKey: _data.IndependentMinimumOddsPercent = integerValue; break;
            case IndependentMaximumOddsKey: _data.IndependentMaximumOddsPercent = integerValue; break;
            case ReplacementBaseOddsKey: _data.ReplacementBaseOddsPercent = integerValue; break;
            case ReplacementSuccessDecreaseKey: _data.ReplacementSuccessDecreasePercent = integerValue; break;
            case ReplacementFailureIncreaseKey: _data.ReplacementFailureIncreasePercent = integerValue; break;
            case ReplacementEliteBonusKey: _data.ReplacementEliteBonusPercent = integerValue; break;
            case ReplacementBossBonusKey: _data.ReplacementBossBonusPercent = integerValue; break;
            case ReplacementMinimumOddsKey: _data.ReplacementMinimumOddsPercent = integerValue; break;
            case ReplacementMaximumOddsKey: _data.ReplacementMaximumOddsPercent = integerValue; break;
            case NormalChoiceCountKey: _data.NormalChoiceCount = integerValue; break;
            case EliteChoiceCountKey: _data.EliteChoiceCount = integerValue; break;
            case BossChoiceCountKey: _data.BossChoiceCount = integerValue; break;
            case NormalGenerationAttemptsKey: _data.NormalGenerationAttempts = integerValue; break;
            case EliteGenerationAttemptsKey: _data.EliteGenerationAttempts = integerValue; break;
            case BossGenerationAttemptsKey: _data.BossGenerationAttempts = integerValue; break;
            case NormalGuaranteedRewardCountKey: _data.NormalGuaranteedRewardCount = integerValue; break;
            case EliteGuaranteedRewardCountKey: _data.EliteGuaranteedRewardCount = integerValue; break;
            case BossGuaranteedRewardCountKey: _data.BossGuaranteedRewardCount = integerValue; break;
            case InitialMysteryPillCountKey: _data.InitialMysteryPillCount = integerValue; break;
            default: return false;
        }

        Normalize();
        return true;
    }

    public static PillRewardRulesSnapshot CreateRulesForNewRun(int maximumChoices)
    {
        Normalize();
        PillRewardRulesSnapshot rules = new()
        {
            IndependentDrop = CreateOdds(
                _data.IndependentBaseOddsPercent,
                _data.IndependentSuccessDecreasePercent,
                _data.IndependentFailureIncreasePercent,
                _data.IndependentEliteBonusPercent,
                _data.IndependentBossBonusPercent,
                _data.IndependentMinimumOddsPercent,
                _data.IndependentMaximumOddsPercent),
            PotionReplacement = CreateOdds(
                _data.ReplacementBaseOddsPercent,
                _data.ReplacementSuccessDecreasePercent,
                _data.ReplacementFailureIncreasePercent,
                _data.ReplacementEliteBonusPercent,
                _data.ReplacementBossBonusPercent,
                _data.ReplacementMinimumOddsPercent,
                _data.ReplacementMaximumOddsPercent),
            NormalChoiceCount = _data.NormalChoiceCount,
            EliteChoiceCount = _data.EliteChoiceCount,
            BossChoiceCount = _data.BossChoiceCount,
            NormalGenerationAttempts = _data.NormalGenerationAttempts,
            EliteGenerationAttempts = _data.EliteGenerationAttempts,
            BossGenerationAttempts = _data.BossGenerationAttempts,
            NormalGuaranteedRewardCount = _data.NormalGuaranteedRewardCount,
            EliteGuaranteedRewardCount = _data.EliteGuaranteedRewardCount,
            BossGuaranteedRewardCount = _data.BossGuaranteedRewardCount,
            InitialMysteryPillCount = _data.InitialMysteryPillCount,
            EnableNormalRewards = _data.EnableNormalRewards,
            EnableEliteRewards = _data.EnableEliteRewards,
            EnableBossRewards = _data.EnableBossRewards,
            PreventDuplicateCapsuleOptionsWithinCombat = _data.PreventDuplicateCapsuleOptionsWithinCombat
        };
        rules.Normalize(maximumChoices);
        return rules;
    }

    private static PillOddsRules CreateOdds(
        int baseOdds,
        int successDecrease,
        int failureIncrease,
        int eliteBonus,
        int bossBonus,
        int minimumOdds,
        int maximumOdds) => new()
    {
        BaseOdds = baseOdds / 100f,
        SuccessDecrease = successDecrease / 100f,
        FailureIncrease = failureIncrease / 100f,
        EliteBonus = eliteBonus / 100f,
        BossBonus = bossBonus / 100f,
        MinimumOdds = minimumOdds / 100f,
        MaximumOdds = maximumOdds / 100f
    };

    private static PillRewardSettingsData CreateDefaultData()
    {
        PillRewardRulesSnapshot defaults = PillRewardDefaults.Create();
        return new PillRewardSettingsData
        {
            IndependentBaseOddsPercent = ToPercent(defaults.IndependentDrop.BaseOdds),
            IndependentSuccessDecreasePercent = ToPercent(defaults.IndependentDrop.SuccessDecrease),
            IndependentFailureIncreasePercent = ToPercent(defaults.IndependentDrop.FailureIncrease),
            IndependentEliteBonusPercent = ToPercent(defaults.IndependentDrop.EliteBonus),
            IndependentBossBonusPercent = ToPercent(defaults.IndependentDrop.BossBonus),
            IndependentMinimumOddsPercent = ToPercent(defaults.IndependentDrop.MinimumOdds),
            IndependentMaximumOddsPercent = ToPercent(defaults.IndependentDrop.MaximumOdds),
            ReplacementBaseOddsPercent = ToPercent(defaults.PotionReplacement.BaseOdds),
            ReplacementSuccessDecreasePercent = ToPercent(defaults.PotionReplacement.SuccessDecrease),
            ReplacementFailureIncreasePercent = ToPercent(defaults.PotionReplacement.FailureIncrease),
            ReplacementEliteBonusPercent = ToPercent(defaults.PotionReplacement.EliteBonus),
            ReplacementBossBonusPercent = ToPercent(defaults.PotionReplacement.BossBonus),
            ReplacementMinimumOddsPercent = ToPercent(defaults.PotionReplacement.MinimumOdds),
            ReplacementMaximumOddsPercent = ToPercent(defaults.PotionReplacement.MaximumOdds),
            NormalChoiceCount = defaults.NormalChoiceCount,
            EliteChoiceCount = defaults.EliteChoiceCount,
            BossChoiceCount = defaults.BossChoiceCount,
            NormalGenerationAttempts = defaults.NormalGenerationAttempts,
            EliteGenerationAttempts = defaults.EliteGenerationAttempts,
            BossGenerationAttempts = defaults.BossGenerationAttempts,
            NormalGuaranteedRewardCount = defaults.NormalGuaranteedRewardCount,
            EliteGuaranteedRewardCount = defaults.EliteGuaranteedRewardCount,
            BossGuaranteedRewardCount = defaults.BossGuaranteedRewardCount,
            InitialMysteryPillCount = defaults.InitialMysteryPillCount,
            SyncMultiplayerParametersWithHost = true,
            EnableSharedMultiplayerPillPool = false,
            EnableNormalRewards = defaults.EnableNormalRewards,
            EnableEliteRewards = defaults.EnableEliteRewards,
            EnableBossRewards = defaults.EnableBossRewards,
            PreventDuplicateCapsuleOptionsWithinCombat = defaults.PreventDuplicateCapsuleOptionsWithinCombat
        };
    }

    private static int ToPercent(float value) => (int)Math.Round(value * 100f, MidpointRounding.AwayFromZero);

    private static void Normalize()
    {
        NormalizeOdds(
            ref _data.IndependentBaseOddsPercent,
            ref _data.IndependentSuccessDecreasePercent,
            ref _data.IndependentFailureIncreasePercent,
            ref _data.IndependentEliteBonusPercent,
            ref _data.IndependentBossBonusPercent,
            ref _data.IndependentMinimumOddsPercent,
            ref _data.IndependentMaximumOddsPercent);
        NormalizeOdds(
            ref _data.ReplacementBaseOddsPercent,
            ref _data.ReplacementSuccessDecreasePercent,
            ref _data.ReplacementFailureIncreasePercent,
            ref _data.ReplacementEliteBonusPercent,
            ref _data.ReplacementBossBonusPercent,
            ref _data.ReplacementMinimumOddsPercent,
            ref _data.ReplacementMaximumOddsPercent);
        _data.NormalChoiceCount = Math.Clamp(_data.NormalChoiceCount, 1, 20);
        _data.EliteChoiceCount = Math.Clamp(_data.EliteChoiceCount, 1, 20);
        _data.BossChoiceCount = Math.Clamp(_data.BossChoiceCount, 1, 20);
        _data.NormalGenerationAttempts = Math.Clamp(_data.NormalGenerationAttempts, 0, 20);
        _data.EliteGenerationAttempts = Math.Clamp(_data.EliteGenerationAttempts, 0, 20);
        _data.BossGenerationAttempts = Math.Clamp(_data.BossGenerationAttempts, 0, 20);
        _data.NormalGuaranteedRewardCount = Math.Clamp(_data.NormalGuaranteedRewardCount, 0, 20);
        _data.EliteGuaranteedRewardCount = Math.Clamp(_data.EliteGuaranteedRewardCount, 0, 20);
        _data.BossGuaranteedRewardCount = Math.Clamp(_data.BossGuaranteedRewardCount, 0, 20);
        _data.InitialMysteryPillCount = Math.Clamp(_data.InitialMysteryPillCount, 0, 1000);
    }

    private static void NormalizeOdds(
        ref int baseOdds,
        ref int successDecrease,
        ref int failureIncrease,
        ref int eliteBonus,
        ref int bossBonus,
        ref int minimumOdds,
        ref int maximumOdds)
    {
        minimumOdds = Math.Clamp(minimumOdds, 0, 100);
        maximumOdds = Math.Clamp(maximumOdds, minimumOdds, 100);
        baseOdds = Math.Clamp(baseOdds, minimumOdds, maximumOdds);
        successDecrease = Math.Clamp(successDecrease, 0, 100);
        failureIncrease = Math.Clamp(failureIncrease, 0, 100);
        eliteBonus = Math.Clamp(eliteBonus, -100, 100);
        bossBonus = Math.Clamp(bossBonus, -100, 100);
    }

    private static bool TryConvertInteger(object? value, out int result)
    {
        try
        {
            if (value is Variant variant)
            {
                return int.TryParse(variant.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
            }

            result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch (Exception)
        {
            result = 0;
            return false;
        }
    }

    private static bool TryConvertBoolean(object? value, out bool result)
    {
        if (value is Variant variant)
        {
            return bool.TryParse(variant.ToString(), out result);
        }

        if (value is bool booleanValue)
        {
            result = booleanValue;
            return true;
        }

        return bool.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out result);
    }

    private static bool IsBooleanKey(string key) => key is
        EnableNormalRewardsKey or EnableEliteRewardsKey or EnableBossRewardsKey or PreventDuplicateOptionsKey or
        EnableSharedMultiplayerPillPoolKey or SyncMultiplayerParametersWithHostKey;

    public const string IndependentBaseOddsKey = "independent_base_odds";
    public const string IndependentSuccessDecreaseKey = "independent_success_decrease";
    public const string IndependentFailureIncreaseKey = "independent_failure_increase";
    public const string IndependentEliteBonusKey = "independent_elite_bonus";
    public const string IndependentBossBonusKey = "independent_boss_bonus";
    public const string IndependentMinimumOddsKey = "independent_minimum_odds";
    public const string IndependentMaximumOddsKey = "independent_maximum_odds";
    public const string ReplacementBaseOddsKey = "replacement_base_odds";
    public const string ReplacementSuccessDecreaseKey = "replacement_success_decrease";
    public const string ReplacementFailureIncreaseKey = "replacement_failure_increase";
    public const string ReplacementEliteBonusKey = "replacement_elite_bonus";
    public const string ReplacementBossBonusKey = "replacement_boss_bonus";
    public const string ReplacementMinimumOddsKey = "replacement_minimum_odds";
    public const string ReplacementMaximumOddsKey = "replacement_maximum_odds";
    public const string NormalChoiceCountKey = "normal_choice_count";
    public const string EliteChoiceCountKey = "elite_choice_count";
    public const string BossChoiceCountKey = "boss_choice_count";
    public const string NormalGenerationAttemptsKey = "normal_generation_attempts";
    public const string EliteGenerationAttemptsKey = "elite_generation_attempts";
    public const string BossGenerationAttemptsKey = "boss_generation_attempts";
    public const string NormalGuaranteedRewardCountKey = "normal_guaranteed_reward_count";
    public const string EliteGuaranteedRewardCountKey = "elite_guaranteed_reward_count";
    public const string BossGuaranteedRewardCountKey = "boss_guaranteed_reward_count";
    public const string InitialMysteryPillCountKey = "initial_mystery_pill_count";
    public const string SyncMultiplayerParametersWithHostKey = "sync_multiplayer_parameters_with_host";
    public const string EnableSharedMultiplayerPillPoolKey = "enable_shared_multiplayer_pill_pool";
    public const string EnableNormalRewardsKey = "enable_normal_rewards";
    public const string EnableEliteRewardsKey = "enable_elite_rewards";
    public const string EnableBossRewardsKey = "enable_boss_rewards";
    public const string PreventDuplicateOptionsKey = "prevent_duplicate_options";

    private static readonly string[] AllKeys =
    [
        IndependentBaseOddsKey, IndependentSuccessDecreaseKey, IndependentFailureIncreaseKey,
        IndependentEliteBonusKey, IndependentBossBonusKey, IndependentMinimumOddsKey, IndependentMaximumOddsKey,
        ReplacementBaseOddsKey, ReplacementSuccessDecreaseKey, ReplacementFailureIncreaseKey,
        ReplacementEliteBonusKey, ReplacementBossBonusKey, ReplacementMinimumOddsKey, ReplacementMaximumOddsKey,
        NormalChoiceCountKey, EliteChoiceCountKey, BossChoiceCountKey,
        NormalGenerationAttemptsKey, EliteGenerationAttemptsKey, BossGenerationAttemptsKey,
        NormalGuaranteedRewardCountKey, EliteGuaranteedRewardCountKey, BossGuaranteedRewardCountKey,
        InitialMysteryPillCountKey,
        SyncMultiplayerParametersWithHostKey,
        EnableSharedMultiplayerPillPoolKey,
        EnableNormalRewardsKey, EnableEliteRewardsKey, EnableBossRewardsKey, PreventDuplicateOptionsKey
    ];
}

public sealed class PillRewardSettingsData
{
    public int IndependentBaseOddsPercent;
    public int IndependentSuccessDecreasePercent;
    public int IndependentFailureIncreasePercent;
    public int IndependentEliteBonusPercent;
    public int IndependentBossBonusPercent;
    public int IndependentMinimumOddsPercent;
    public int IndependentMaximumOddsPercent;
    public int ReplacementBaseOddsPercent;
    public int ReplacementSuccessDecreasePercent;
    public int ReplacementFailureIncreasePercent;
    public int ReplacementEliteBonusPercent;
    public int ReplacementBossBonusPercent;
    public int ReplacementMinimumOddsPercent;
    public int ReplacementMaximumOddsPercent;
    public int NormalChoiceCount;
    public int EliteChoiceCount;
    public int BossChoiceCount;
    public int NormalGenerationAttempts;
    public int EliteGenerationAttempts;
    public int BossGenerationAttempts;
    public int NormalGuaranteedRewardCount;
    public int EliteGuaranteedRewardCount;
    public int BossGuaranteedRewardCount;
    public int InitialMysteryPillCount;
    public bool SyncMultiplayerParametersWithHost;
    public bool EnableSharedMultiplayerPillPool;
    public bool EnableNormalRewards;
    public bool EnableEliteRewards;
    public bool EnableBossRewards;
    public bool PreventDuplicateCapsuleOptionsWithinCombat;
}
