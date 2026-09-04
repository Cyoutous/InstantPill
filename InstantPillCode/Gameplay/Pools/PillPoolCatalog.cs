using System.Collections.Generic;
using System.Linq;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// The static set of card model entries eligible for InstantPill's run-local pools.
/// Runtime state stores only entries selected from these lists.
/// </summary>
public static class PillPoolCatalog
{
    public static IReadOnlyList<string> MysteryPillIds { get; } = CreateIds("MYSTERY", 20);

    public static IReadOnlyList<string> EffectPillIds { get; } =
        CreateIds("EFFECT", 30)
            .Append("INSTANTPILL-I_FOUND_PILLS")
            .ToArray();

    private static string[] CreateIds(string family, int count) =>
        Enumerable.Range(1, count)
            .Select(index => $"INSTANTPILL-{family}_PILL_{index:D3}")
            .ToArray();
}
