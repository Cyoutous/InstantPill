using System.Reflection;
using BaseLib.Config;
using Godot;
using HarmonyLib;
using InstantPill.InstantPillCode.Debug;
using MegaCrit.Sts2.Core.Modding;

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

        //If you want to use scripts defined in your mod for Godot scenes, uncomment the following line.
        //Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(assembly);
     
        ModConfigRegistry.Register(ModId, new InstantPillDebugConfig());

        Harmony harmony = new(ModId);

        harmony.PatchAll(assembly);
        Logger.Info("Initialized custom Capsule keyword and start-of-combat handler.", 1);
    }
}
