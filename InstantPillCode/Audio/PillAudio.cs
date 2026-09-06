using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using MegaCrit.Sts2.Core.Saves;

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
    /// Preloads a mod-owned raw audio asset once, then plays it as a local FMOD one-shot.
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

        if (_preloadAsSound is not null && PreloadedPaths.Add(resourcePath))
        {
            Invoke(_preloadAsSound, resourcePath, "preload");
        }

        if (_playSoundFile is not null)
        {
            // VolumeSfx is the 0-1 value driven by the game's native SFX-volume slider.
            // Reading it here keeps every new capsule sound aligned with the player's current setting.
            float actualVolume = baseVolume * Math.Clamp(SaveManager.Instance.SettingsSave.VolumeSfx, 0f, 1f);
            Invoke(_playSoundFile, resourcePath, "play", actualVolume, pitch);
        }
    }

    private static void ResolveStreamingMethods()
    {
        if (_preloadAsSound is not null && _playSoundFile is not null)
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

        _preloadAsSound = FindStringMethod(streamingFilesType, "TryPreloadAsSound", parameterCount: 1);
        _playSoundFile = FindStringMethod(streamingFilesType, "TryPlaySoundFile");

        if (_preloadAsSound is null || _playSoundFile is null)
        {
            ReportUnavailable("RitsuLib's installed audio API does not expose the required sound methods.");
            return;
        }

        if (!_reportedReady)
        {
            _reportedReady = true;
            MainFile.Logger.Info("InstantPill connected to RitsuLib's FMOD audio bridge.", 1);
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

    private static void Invoke(
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

            method.Invoke(null, arguments);
        }
        catch (TargetInvocationException exception)
        {
            MainFile.Logger.Warn($"InstantPill could not {operation} audio '{path}': {exception.InnerException?.Message ?? exception.Message}");
        }
        catch (Exception exception)
        {
            MainFile.Logger.Warn($"InstantPill could not {operation} audio '{path}': {exception.Message}");
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
