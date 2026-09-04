namespace InstantPill.InstantPillCode.Gameplay.Effects;

/// <summary>Registers Puberty's primitive per-player counter with BaseLib's save extension.</summary>
public static class PubertySaveRegistration
{
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        if (!PubertyPlayCounter.Count.RegisterCustomSave())
        {
            throw new System.InvalidOperationException("InstantPill could not register its Puberty play-count save field.");
        }

        _registered = true;
    }
}
