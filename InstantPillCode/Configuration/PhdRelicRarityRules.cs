using MegaCrit.Sts2.Core.Entities.Relics;

namespace InstantPill.InstantPillCode.Configuration;

/// <summary>
/// Stable persisted values for PHD's configurable relic pool. Keep these strings independent
/// from localized UI labels: they are stored in the global settings file and run snapshots.
/// </summary>
public static class PhdRelicRarityRules
{
    public const string Common = "common";
    public const string Uncommon = "uncommon";
    public const string Rare = "rare";
    public const string Shop = "shop";
    public const string None = "none";

    public const string DefaultValue = Shop;

    public static string Normalize(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        Common => Common,
        Uncommon => Uncommon,
        Rare => Rare,
        Shop => Shop,
        None => None,
        _ => DefaultValue
    };

    public static RelicRarity ToRelicRarity(string? value) => Normalize(value) switch
    {
        Common => RelicRarity.Common,
        Uncommon => RelicRarity.Uncommon,
        Rare => RelicRarity.Rare,
        Shop => RelicRarity.Shop,
        _ => RelicRarity.None
    };
}
