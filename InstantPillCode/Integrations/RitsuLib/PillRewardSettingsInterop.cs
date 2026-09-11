using System.Globalization;
using InstantPill.InstantPillCode.Configuration;

namespace InstantPill.InstantPillCode.Integrations.RitsuLib;

/// <summary>
/// Schema-based RitsuLib settings provider. It intentionally has no RitsuLib assembly reference:
/// RitsuLib discovers it from assembly metadata only when the framework is installed.
/// </summary>
public static class PillRewardSettingsInterop
{
    public static object CreateRitsuLibSettingsSchema() => "res://InstantPill/ritsulib/reward_settings_schema.json";

    public static object? GetRitsuLibSettingValue(string key) => PillRewardSettings.GetValue(key);

    public static void SetRitsuLibSettingValue(string key, object? value)
        => SetValue(key, value);

    // Current RitsuLib runtime interop selects these typed methods for integer sliders and
    // toggles before consulting the object channel. Keep both channels so the provider remains
    // compatible with earlier schema-interoperability implementations as well.
    public static int GetRitsuLibSettingInt(string key) => PillRewardSettings.GetValue(key) is int value ? value : 0;

    public static void SetRitsuLibSettingInt(string key, int value) => SetValue(key, value);

    // RitsuLib renders both text boxes and choice controls through its string channel. The
    // starting-pill text box remains numeric internally; PHD rarity is a stable string value.
    public static string GetRitsuLibSettingString(string key) =>
        PillRewardSettings.GetValue(key) switch
        {
            int value => value.ToString(CultureInfo.InvariantCulture),
            string value => value,
            _ => string.Empty
        };

    public static void SetRitsuLibSettingString(string key, string value) => SetValue(key, value);

    public static bool GetRitsuLibSettingBool(string key) => PillRewardSettings.GetValue(key) is bool value && value;

    public static void SetRitsuLibSettingBool(string key, bool value) => SetValue(key, value);

    private static void SetValue(string key, object? value)
    {
        if (PillRewardSettings.TrySet(key, value))
        {
            // Runtime-schema settings are not guaranteed to receive a deferred save callback
            // on every RitsuLib version. Persist immediately so a slider change survives restart.
            PillRewardSettings.Save();
            return;
        }

        MainFile.Logger.Warn($"InstantPill ignored invalid RitsuLib setting '{key}'.");
    }

    public static void SaveRitsuLibSettings() => PillRewardSettings.Save();

    public static void InvokeRitsuLibSettingAction(string key)
    {
        if (key == "save_settings")
        {
            PillRewardSettings.Save();
            return;
        }

        if (key != "reset_defaults")
        {
            return;
        }

        PillRewardSettings.ResetToDefaults();
        PillRewardSettings.Save();
    }
}
