using BaseLib.Patches.Saves;

namespace InstantPill.InstantPillCode.Gameplay.Rewards;

/// <summary>Registers InstantPill's reward pity-state save field with BaseLib.</summary>
public static class PillRewardSaveRegistration
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        ExtendedSaveTypes.RegisterObjectSaveType<PillOddsRules>(
            ExtendedSaveTypes.PropertyFunc<PillOddsRules, float>(nameof(PillOddsRules.BaseOdds)),
            ExtendedSaveTypes.PropertyFunc<PillOddsRules, float>(nameof(PillOddsRules.SuccessDecrease)),
            ExtendedSaveTypes.PropertyFunc<PillOddsRules, float>(nameof(PillOddsRules.FailureIncrease)),
            ExtendedSaveTypes.PropertyFunc<PillOddsRules, float>(nameof(PillOddsRules.EliteBonus)),
            ExtendedSaveTypes.PropertyFunc<PillOddsRules, float>(nameof(PillOddsRules.BossBonus)),
            ExtendedSaveTypes.PropertyFunc<PillOddsRules, float>(nameof(PillOddsRules.MinimumOdds)),
            ExtendedSaveTypes.PropertyFunc<PillOddsRules, float>(nameof(PillOddsRules.MaximumOdds)));
        ExtendedSaveTypes.RegisterObjectSaveType<PillRewardRulesSnapshot>(
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, PillOddsRules>(nameof(PillRewardRulesSnapshot.IndependentDrop)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, PillOddsRules>(nameof(PillRewardRulesSnapshot.PotionReplacement)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.NormalChoiceCount)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.EliteChoiceCount)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.BossChoiceCount)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.NormalGenerationAttempts)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.EliteGenerationAttempts)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.BossGenerationAttempts)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.NormalGuaranteedRewardCount)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.EliteGuaranteedRewardCount)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, int>(nameof(PillRewardRulesSnapshot.BossGuaranteedRewardCount)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, bool>(nameof(PillRewardRulesSnapshot.EnableNormalRewards)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, bool>(nameof(PillRewardRulesSnapshot.EnableEliteRewards)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, bool>(nameof(PillRewardRulesSnapshot.EnableBossRewards)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRulesSnapshot, bool>(nameof(PillRewardRulesSnapshot.PreventDuplicateCapsuleOptionsWithinCombat)));
        ExtendedSaveTypes.RegisterObjectSaveType<PillRewardRunState>(
            ExtendedSaveTypes.PropertyFunc<PillRewardRunState, int>(nameof(PillRewardRunState.SchemaVersion)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRunState, float>(nameof(PillRewardRunState.IndependentCurrentOdds)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRunState, float>(nameof(PillRewardRunState.ReplacementCurrentOdds)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRunState, int>(nameof(PillRewardRunState.IndependentRollCounter)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRunState, int>(nameof(PillRewardRunState.ReplacementRollCounter)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRunState, int>(nameof(PillRewardRunState.ChoiceRollCounter)),
            ExtendedSaveTypes.PropertyFunc<PillRewardRunState, PillRewardRulesSnapshot>(nameof(PillRewardRunState.Rules)));

        if (!PillRewardService.State.RegisterCustomSave())
        {
            throw new System.InvalidOperationException("InstantPill could not register its per-player reward save field.");
        }

        _registered = true;
    }
}
