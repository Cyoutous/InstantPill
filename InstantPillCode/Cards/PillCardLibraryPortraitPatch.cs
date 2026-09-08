using HarmonyLib;
using InstantPill.InstantPillCode.Content;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace InstantPill.InstantPillCode.Cards;

/// <summary>
/// Supplies purely cosmetic, session-local portraits for identified pills while they are viewed
/// through the card library's inspect screen.  Combat portraits remain controlled exclusively by
/// the run's mystery-to-effect mapping.
/// </summary>
internal static class PillCardLibraryPortraitContext
{
    private static readonly Dictionary<string, string> PortraitsByEffectId = new(StringComparer.Ordinal);
    private static bool _libraryOpenPending;
    private static bool _libraryInspectActive;

    public static void MarkCardLibraryOpenAttempt() => _libraryOpenPending = true;

    public static void CancelCardLibraryOpenAttempt() => _libraryOpenPending = false;

    public static void BeginInspectSession()
    {
        _libraryInspectActive = _libraryOpenPending;
        _libraryOpenPending = false;
        PortraitsByEffectId.Clear();
    }

    public static void EndInspectSession()
    {
        _libraryInspectActive = false;
        _libraryOpenPending = false;
        PortraitsByEffectId.Clear();
    }

    public static bool TryGetPortrait(string effectPillId, out string portraitPath)
    {
        if (!_libraryInspectActive)
        {
            portraitPath = string.Empty;
            return false;
        }

        if (!PortraitsByEffectId.TryGetValue(effectPillId, out portraitPath!))
        {
            IReadOnlyList<string> candidates = PillPortraitCatalog.AllMysteryPortraitPaths;
            if (candidates.Count == 0)
            {
                portraitPath = PillPortraitCatalog.DefaultEffectPortraitPath;
            }
            else
            {
                portraitPath = candidates[Random.Shared.Next(candidates.Count)];
            }

            PortraitsByEffectId.Add(effectPillId, portraitPath);
        }

        return true;
    }
}

/// <summary>
/// Restricts the random-library portrait context to NCardLibrary. Other uses of the common
/// inspect screen (deck view, rewards, history) keep their normal, deterministic portrait path.
/// </summary>
[HarmonyPatch]
internal static class PillCardLibraryPortraitPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(NCardLibrary), "ShowCardDetail")]
    private static void MarkCardLibraryOpenAttempt()
    {
        PillCardLibraryPortraitContext.MarkCardLibraryOpenAttempt();
    }

    [HarmonyFinalizer]
    [HarmonyPatch(typeof(NCardLibrary), "ShowCardDetail")]
    private static Exception? CancelUnconsumedCardLibraryOpenAttempt(Exception? __exception)
    {
        PillCardLibraryPortraitContext.CancelCardLibraryOpenAttempt();
        return __exception;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NInspectCardScreen), nameof(NInspectCardScreen.Open))]
    private static void BeginInspectSession()
    {
        PillCardLibraryPortraitContext.BeginInspectSession();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(NInspectCardScreen), nameof(NInspectCardScreen.Close))]
    private static void EndInspectSession()
    {
        PillCardLibraryPortraitContext.EndInspectSession();
    }
}
