using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Nodes.Audio;

namespace InstantPill.InstantPillCode.Audio;

/// <summary>
/// Plays mod-owned one-shot sounds through RitsuLib when it is present.
///
/// RitsuLib remains an optional dependency: the rest of InstantPill must work
/// normally when its assembly is not loaded.
/// </summary>
internal static class PillAudio
{
    private const string StreamingFilesTypeName = "STS2RitsuLib.Audio.FmodStudioStreamingFiles";

    // Packed mod assets must use RitsuLib's resource-specific bridge. It materializes the
    // imported audio resource from the PCK into a private FMOD-readable cache before playback.
    private static MethodInfo? _preloadResourceAsSound;
    private static MethodInfo? _playResourceSound;

    // Retain RitsuLib's loose-file API as a fallback for older installed versions which predate
    // the resource bridge.
    private static MethodInfo? _preloadAsSound;
    private static MethodInfo? _playSoundFile;
    private static readonly HashSet<string> PreloadedPaths = new(StringComparer.Ordinal);
    private static bool _reportedUnavailable;
    private static bool _reportedReady;
    private static readonly AsyncLocal<int> CustomSoundSuppressionDepth = new();

    /// <summary>
    /// Whether this async execution flow should omit sounds authored by InstantPill cards.
    /// Native game VFX and SFX are deliberately outside this scope.
    /// </summary>
    public static bool AreCustomCardSoundsSuppressed => CustomSoundSuppressionDepth.Value > 0;

    /// <summary>
    /// Temporarily silences only InstantPill-authored card audio on the current async flow.
    /// This is used when Question Marks executes another pill's gameplay as a proxy.
    /// </summary>
    public static IDisposable SuppressCustomCardSounds()
    {
        CustomSoundSuppressionDepth.Value++;
        return new CustomSoundSuppressionScope();
    }

    /// <summary>Finds the optional RitsuLib FMOD bridge without preloading any individual asset.</summary>
    public static void Initialize()
    {
        ResolveStreamingMethods();
    }

    /// <summary>
    /// Preloads a mod-owned audio asset once, then plays it as a local FMOD one-shot.
    /// The caller owns the resource path and decides when playback is appropriate.
    /// </summary>
    public static void PlayOneShot(string resourcePath, float baseVolume = 1f, float pitch = 1f)
    {
        if (AreCustomCardSoundsSuppressed)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            MainFile.Logger.Warn("InstantPill ignored an audio request with an empty resource path.");
            return;
        }

        Initialize();

        bool isGodotResource = resourcePath.StartsWith("res://", StringComparison.Ordinal) ||
                               resourcePath.StartsWith("user://", StringComparison.Ordinal);
        MethodInfo? preloadMethod = isGodotResource && _preloadResourceAsSound is not null
            ? _preloadResourceAsSound
            : _preloadAsSound;
        MethodInfo? playMethod = isGodotResource && _playResourceSound is not null
            ? _playResourceSound
            : _playSoundFile;

        if (preloadMethod is not null && PreloadedPaths.Add(resourcePath))
        {
            if (!Invoke(preloadMethod, resourcePath, "preload"))
            {
                // A failed cache materialization must be retried on a later request instead of
                // being permanently treated as preloaded for this game session.
                PreloadedPaths.Remove(resourcePath);
            }
        }

        if (playMethod is not null)
        {
            // VolumeSfx is the 0-1 value driven by the game's native SFX-volume slider.
            // Reading it here keeps every new capsule sound aligned with the player's current setting.
            float actualVolume = baseVolume * Math.Clamp(SaveManager.Instance.SettingsSave.VolumeSfx, 0f, 1f);
            Invoke(playMethod, resourcePath, "play", actualVolume, pitch);
        }
    }

    /// <summary>
    /// Plays a built-in STS2 FMOD event immediately. This deliberately does not consult
    /// <see cref="AreCustomCardSoundsSuppressed"/>: callers use it for authored card-effect
    /// audio which remains audible when Question Marks proxies a pill's gameplay.
    /// </summary>
    public static void PlayVanillaCardEffect(string eventPath)
    {
        if (string.IsNullOrWhiteSpace(eventPath))
        {
            MainFile.Logger.Warn("InstantPill ignored a vanilla audio request with an empty event path.");
            return;
        }

        try
        {
            // Native event routing retains the game's own SFX-bus and volume-slider behavior.
            NAudioManager? audioManager = NAudioManager.Instance;
            if (audioManager is null)
            {
                MainFile.Logger.Warn($"InstantPill could not play vanilla audio event '{eventPath}': audio manager is unavailable.");
                return;
            }

            audioManager.PlayOneShot(eventPath);
        }
        catch (Exception exception)
        {
            MainFile.Logger.Warn($"InstantPill could not play vanilla audio event '{eventPath}': {exception.Message}");
        }
    }

    private static void ResolveStreamingMethods()
    {
        if ((_preloadResourceAsSound is not null && _playResourceSound is not null) ||
            (_preloadAsSound is not null && _playSoundFile is not null))
        {
            return;
        }

        Type? streamingFilesType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(StreamingFilesTypeName, throwOnError: false))
            .FirstOrDefault(type => type is not null);

        if (streamingFilesType is null)
        {
            ReportUnavailable("RitsuLib's audio bridge is not loaded.");
            return;
        }

        _preloadResourceAsSound = FindStringMethod(
            streamingFilesType,
            "TryPreloadResourceAsSound",
            parameterCount: 1);
        _playResourceSound = FindStringMethod(streamingFilesType, "TryPlayResourceSound");

        _preloadAsSound = FindStringMethod(streamingFilesType, "TryPreloadAsSound", parameterCount: 1);
        _playSoundFile = FindStringMethod(streamingFilesType, "TryPlaySoundFile");

        bool hasResourceBridge = _preloadResourceAsSound is not null && _playResourceSound is not null;
        bool hasLooseFileBridge = _preloadAsSound is not null && _playSoundFile is not null;
        if (!hasResourceBridge && !hasLooseFileBridge)
        {
            ReportUnavailable("RitsuLib's installed audio API does not expose the required sound methods.");
            return;
        }

        if (!_reportedReady)
        {
            _reportedReady = true;
            string bridgeKind = hasResourceBridge ? "resource" : "loose-file fallback";
            MainFile.Logger.Info($"InstantPill connected to RitsuLib's {bridgeKind} FMOD audio bridge.", 1);
        }
    }

    private static MethodInfo? FindStringMethod(Type type, string name, int? parameterCount = null)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(method =>
        {
            ParameterInfo[] parameters = method.GetParameters();
            return method.Name == name
                && parameters.Length > 0
                && parameters[0].ParameterType == typeof(string)
                && (!parameterCount.HasValue || parameters.Length == parameterCount.Value)
                && parameters.Skip(1).All(parameter => parameter.IsOptional);
        });
    }

    private static bool Invoke(
        MethodInfo method,
        string path,
        string operation,
        float? volume = null,
        float? pitch = null)
    {
        try
        {
            ParameterInfo[] parameters = method.GetParameters();
            object?[] arguments = new object?[parameters.Length];
            arguments[0] = path;

            for (int i = 1; i < arguments.Length; i++)
            {
                arguments[i] = Type.Missing;
            }

            if (volume.HasValue && arguments.Length > 1)
            {
                arguments[1] = volume.Value;
            }

            if (pitch.HasValue && arguments.Length > 2)
            {
                arguments[2] = pitch.Value;
            }

            object? result = method.Invoke(null, arguments);
            if (result is bool success && !success)
            {
                MainFile.Logger.Warn($"InstantPill audio {operation} returned false for '{path}'.");
                return false;
            }

            return true;
        }
        catch (TargetInvocationException exception)
        {
            MainFile.Logger.Warn($"InstantPill could not {operation} audio '{path}': {exception.InnerException?.Message ?? exception.Message}");
            return false;
        }
        catch (Exception exception)
        {
            MainFile.Logger.Warn($"InstantPill could not {operation} audio '{path}': {exception.Message}");
            return false;
        }
    }

    private static void ReportUnavailable(string message)
    {
        if (_reportedUnavailable)
        {
            return;
        }

        _reportedUnavailable = true;
        MainFile.Logger.Warn($"InstantPill audio is disabled: {message}");
    }

    private sealed class CustomSoundSuppressionScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            CustomSoundSuppressionDepth.Value = Math.Max(0, CustomSoundSuppressionDepth.Value - 1);
        }
    }
}
