using RingLib.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RingLib;

internal class Mod : Modding.Mod
{
    public static Mod Instance { get; private set; }
    private string version;
    private static List<(string, string)> internalPreloadNames = [
        ("GG_Sly", "Battle Scene/Sly Boss/S1")
    ];
    private List<(string, string)> preloadNames;
    private static Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;

    public Mod(string name, string version, List<(string, string)> preloadNames) : base(name)
    {
        Instance = this;
        this.version = version;
        this.preloadNames = internalPreloadNames.Concat(preloadNames).ToList();
#if DEBUG
        RingLib.Log.LoggerInfo = Log;
#endif
        RingLib.Log.LoggerError = LogError;
    }

    public sealed override string GetVersion() => version;

    public sealed override List<(string, string)> GetPreloadNames() => preloadNames;

    private static void HeroControllerRegainControl(On.HeroController.orig_RegainControl orig, HeroController self)
    {
        var control = self.gameObject.GetComponent<Control>();
        if (control != null && control.HasControlled)
        {
            return;
        }
        orig(self);
    }

    private static void InstallHooks()
    {
        On.HeroController.RegainControl += HeroControllerRegainControl;
    }

    public virtual void ModStart() { }

    public sealed override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Mod.preloadedObjects = preloadedObjects;
        InstallHooks();
        ModStart();
    }

    public static void StaticInitialize(Action<string> loggerInfo, Action<string> loggerError,
    Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
#if DEBUG
        RingLib.Log.LoggerInfo = loggerInfo;
#endif
        RingLib.Log.LoggerError = loggerError;
        Mod.preloadedObjects = preloadedObjects;
        InstallHooks();
    }

    public static GameObject GetPreloaded(string scene, string name)
    {
        if (!preloadedObjects.ContainsKey(scene) || !preloadedObjects[scene].ContainsKey(name))
        {
            RingLib.Log.LogError(typeof(Mod).Name, $"Preloaded object {scene}/{name} not found");
            return null;
        }
        return preloadedObjects[scene][name];
    }
}
