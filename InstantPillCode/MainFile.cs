using System.Reflection;
using Godot;
using HarmonyLib;
using InstantPill.InstantPillCode.Audio;
using InstantPill.InstantPillCode.Configuration;
using InstantPill.InstantPillCode.Gameplay.Effects;
using InstantPill.InstantPillCode.Gameplay.Pools;
using InstantPill.InstantPillCode.Gameplay.Rewards;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Runs;

namespace InstantPill.InstantPillCode;

//You're recommended but not required to keep all your code in this package and all your assets in the InstantPill folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "InstantPill"; //At the moment, this is used only for the Logger and harmony names.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        PillRewardSettings.Load();
        PillAudio.Initialize();

        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);
     
        PillPoolSaveRegistration.Register();
        PillRewardSaveRegistration.Register();
        PubertySaveRegistration.Register();
        RunManager.Instance.RunStarted += InitializePillRunState;

        Harmony harmony = new(ModId);

        harmony.PatchAll(assembly);
        Logger.Info("Initialized InstantPill keywords, combat handlers, pools, and reward odds.", 1);
    }

    private static void InitializePillRunState(RunState runState)
    {
        foreach (var player in runState.Players)
        {
            PillPoolService.EnsureInitialized(player);
            PillRewardService.EnsureInitialized(player);
        }
    }
}
