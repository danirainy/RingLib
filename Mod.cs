using RingLib.Components;
using UnityEngine;

namespace RingLib;

internal class Mod : Modding.Mod
{
    public static Mod Instance { get; private set; }
    private string version;
    private List<(string, string)> preloadNames;
    private static Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;

    public Mod(string name, string version, List<(string, string)> preloadNames) : base(name)
    {
        Instance = this;
        this.version = version;
        var internalPreloadNames = new List<(string, string)>
        {
            ("GG_Sly", "Battle Scene")
        };
        this.preloadNames = internalPreloadNames.Concat(preloadNames).ToList();
#if DEBUG
        RingLib.Log.LoggerInfo = Log;
#endif
        RingLib.Log.LoggerError = LogError;
    }

    public sealed override string GetVersion() => version;

    public sealed override List<(string, string)> GetPreloadNames() => preloadNames;

    public virtual void ModStart() { }

    private static void HeroControllerRegainControl(On.HeroController.orig_RegainControl orig, HeroController self)
    {
        var control = self.gameObject.GetComponent<Control>();
        if (control != null && control.HasControlled)
        {
            return;
        }
        orig(self);
    }

    public sealed override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Mod.preloadedObjects = preloadedObjects;
        On.HeroController.RegainControl += HeroControllerRegainControl;
        ModStart();
    }

    public static GameObject GetPreloaded(string path)
    {
        var parts = path.Split('/');
        var root = preloadedObjects[parts[0]][parts[1]];
        for (int i = 2; i < parts.Length; ++i)
        {
            root = root.transform.Find(parts[i]).gameObject;
        }
        return root;
    }
}
