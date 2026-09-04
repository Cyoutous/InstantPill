using System;
using System.Linq;
using System.Reflection;

namespace InstantPill.InstantPillCode.Audio;

/// <summary>
/// Plays mod-owned one-shot sounds through RitsuLib when it is present.
///
/// RitsuLib remains an optional dependency: the rest of InstantPill must work
/// normally when its assembly is not loaded.
/// </summary>
internal static class PillAudio
{
    public const string IFoundPillsPath = "res://audio/i found pills 3.wav";

    private const string StreamingFilesTypeName = "STS2RitsuLib.Audio.FmodStudioStreamingFiles";

    private static MethodInfo? _preloadAsSound;
    private static MethodInfo? _playSoundFile;
    private static bool _preloadRequested;
    private static bool _reportedUnavailable;
    private static bool _reportedReady;

    /// <summary>Preloads the one-shot sound when RitsuLib's FMOD bridge is available.</summary>
    public static void Initialize()
    {
        ResolveStreamingMethods();

        if (_preloadRequested || _preloadAsSound is null)
        {
            return;
        }

        _preloadRequested = true;
        Invoke(_preloadAsSound, IFoundPillsPath, "preload");
    }

    /// <summary>Plays the I Found Pills one-shot for the local player.</summary>
    public static void PlayIFoundPills()
    {
        Initialize();

        if (_playSoundFile is not null)
        {
            Invoke(_playSoundFile, IFoundPillsPath, "play");
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

    private static void Invoke(MethodInfo method, string path, string operation)
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
}
