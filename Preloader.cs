using UnityEngine;

namespace RingLib;

internal class Preloader
{
    public static List<(string, string)> PreloadNames = new List<(string, string)>
    {
        ("GG_Sly", "Battle Scene"),
    };
    private static Dictionary<string, Dictionary<string, GameObject>> preloadedObjects;
    public static List<(string, string)> GetPreloadNames()
    {
        return PreloadNames;
    }
    public static void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Preloader.preloadedObjects = preloadedObjects;
    }
    public static GameObject Get(string path)
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
