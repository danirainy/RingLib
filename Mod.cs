using Modding;
using RingLib.Components;
using RingLib.StateMachine;
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
        ("GG_Sly", "Battle Scene/Sly Boss/S1"),
        ("GG_Grey_Prince_Zote", "Grey Prince")
    ];
    private List<(string, string)> preloadNames;
    private static Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;
    private List<string> dependencies;

    public Mod(string name, string version, List<(string, string)> preloadNames,
        List<string> dependencies) : base(name)
    {
        Instance = this;
#if DEBUG
        RingLib.Log.LoggerInfo = Log;
#endif
        RingLib.Log.LoggerError = LogError;
        this.version = version;
        this.preloadNames = internalPreloadNames.Concat(preloadNames).ToList();
        this.dependencies = dependencies;
    }

    public sealed override string GetVersion() => version;

    public sealed override List<(string, string)> GetPreloadNames() => preloadNames;

    public virtual string Translate(string key)
    {
        return null;
    }

    private static void InstallHooks()
    {
        On.HeroController.RegainControl += (orig, self) =>
        {
            var control = self.gameObject.GetComponent<Control>();
            if (control != null && control.HasControlled)
            {
                return;
            }
            orig(self);
        };

        On.HealthManager.TakeDamage += (orig, self, damage) =>
        {
            var entityStateMachine = self.gameObject.GetComponent<EntityStateMachine>();
            if (entityStateMachine != null)
            {
                entityStateMachine.OnHit();
            }
            orig(self, damage);
        };

        On.HealthManager.SendDeathEvent += (orig, self) =>
        {
            var entityStateMachine = self.gameObject.GetComponent<EntityStateMachine>();
            if (entityStateMachine != null)
            {
                entityStateMachine.OnDeath();
            }
            orig(self);
        };

        ModHooks.LanguageGetHook += (key, sheetTitle, orig) =>
        {
            var translation = Instance.Translate(key);
            if (translation != null)
            {
                return translation;
            }
            return orig;
        };
    }

    public virtual void ModStart() { }

    public sealed override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Mod.preloadedObjects = preloadedObjects;
        List<string> missing = dependencies.Where(dependency => ModHooks.GetMod(dependency) == null).ToList();
        if (missing.Count > 0)
        {
            throw new Exception($"Missing dependencies: {string.Join(", ", missing)}");
        }
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
