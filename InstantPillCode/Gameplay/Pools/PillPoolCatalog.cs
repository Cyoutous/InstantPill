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
            .Append("INSTANTPILL-PUBERTY")
            .Append("INSTANTPILL-RE_LAX")
            .Append("INSTANTPILL-BAD_GAS")
            .Append("INSTANTPILL-EXPLOSIVE_DIARRHEA")
            .Append("INSTANTPILL-TELEPILLS")
            .Append("INSTANTPILL-PARALYSIS")
            .Append("INSTANTPILL-PHEROMONES")
            .Append("INSTANTPILL-LEMON_PARTY")
            .Append("INSTANTPILL-R_U_A_WIZARD")
            .Append("INSTANTPILL-PERCS")
            .Append("INSTANTPILL-ADDICTED")
            .Append("INSTANTPILL-QUESTION_MARKS")
            .Append("INSTANTPILL-ONE_MAKES_YOU_LARGER")
            .Append("INSTANTPILL-ONE_MAKES_YOU_SMALL")
            .Append("INSTANTPILL-INFESTED_1")
            .Append("INSTANTPILL-INFESTED_2")
            .Append("INSTANTPILL-POWER_PILL")
            .Append("INSTANTPILL-RETRO_VISION")
            .Append("INSTANTPILL-FRIEND_TILL_THE_END")
            .Append("INSTANTPILL-X_LAX")
            .Append("INSTANTPILL-SOMETHINGS_WRONG")
            .Append("INSTANTPILL-IM_DROWSY")
            .Append("INSTANTPILL-IM_EXCITED")
            .Append("INSTANTPILL-SUNSHINE")
            .Append("INSTANTPILL-BAD_TRIP")
            .Append("INSTANTPILL-BALLS_OF_STEEL")
            .Append("INSTANTPILL-FULL_HEALTH")
            .Append("INSTANTPILL-PRETTY_FLY")
            .Append("INSTANTPILL-48_HOUR_ENERGY")
            .Append("INSTANTPILL-HEMATEMESIS")
            .Append("INSTANTPILL-I_CAN_SEE_FOREVER")
            .Append("INSTANTPILL-AMNESIA")
            .Append("INSTANTPILL-GULP")
            .Append("INSTANTPILL-HURF")
            .Append("INSTANTPILL-VURP")
            .Append("INSTANTPILL-HEALTH_DOWN")
            .Append("INSTANTPILL-HEALTH_UP")
            .Append("INSTANTPILL-SPEED_DOWN")
            .Append("INSTANTPILL-SPEED_UP")
            .Append("INSTANTPILL-LUCK_DOWN")
            .Append("INSTANTPILL-LUCK_UP")
            .Append("INSTANTPILL-EXPERIMENTAL_PILL")
            .ToArray();

    private static string[] CreateIds(string family, int count) =>
        Enumerable.Range(1, count)
            .Select(index => $"INSTANTPILL-{family}_PILL_{index:D3}")
            .ToArray();
}
