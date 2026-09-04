using BaseLib.Patches.Saves;

namespace InstantPill.InstantPillCode.Gameplay.Pools;

/// <summary>
/// Registers the pool state and its nested collection types with BaseLib's save extension.
/// </summary>
public static class PillPoolSaveRegistration
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        ExtendedSaveTypes.RegisterObjectSaveType<PillPoolSlotState>(
            ExtendedSaveTypes.PropertyFunc<PillPoolSlotState, string>(nameof(PillPoolSlotState.MysteryPillId)),
            ExtendedSaveTypes.PropertyFunc<PillPoolSlotState, string>(nameof(PillPoolSlotState.AssignedEffectPillId)),
            ExtendedSaveTypes.PropertyFunc<PillPoolSlotState, bool>(nameof(PillPoolSlotState.IsRevealed)));
        ExtendedSaveTypes.RegisterListSaveType<PillPoolSlotState>();
        ExtendedSaveTypes.RegisterListSaveType<string>();
        ExtendedSaveTypes.RegisterObjectSaveType<PillPoolState>(
            ExtendedSaveTypes.PropertyFunc<PillPoolState, int>(nameof(PillPoolState.SchemaVersion)),
            ExtendedSaveTypes.PropertyFunc<PillPoolState, int>(nameof(PillPoolState.RandomRollCounter)),
            ExtendedSaveTypes.PropertyFunc<PillPoolState, System.Collections.Generic.List<string>>(nameof(PillPoolState.RemainingEffectIds)),
            ExtendedSaveTypes.PropertyFunc<PillPoolState, System.Collections.Generic.List<PillPoolSlotState>>(nameof(PillPoolState.CapsuleSlots)));

        if (!PillPoolService.State.RegisterCustomSave())
        {
            throw new System.InvalidOperationException("InstantPill could not register its per-player pool save field.");
        }

        _registered = true;
    }
}
