using System.Reflection;
using UnityEngine;

namespace RingLib;

internal static class ComponentPatcher<T> where T : Component
{
    private static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
    private static PropertyInfo[] properties = typeof(T).GetProperties(flags);
    private static Dictionary<string, PropertyInfo> propertyDict = properties.ToDictionary(prop => prop.Name);
    private static FieldInfo[] fields = typeof(T).GetFields(flags);
    private static Dictionary<string, FieldInfo> fieldDict = fields.ToDictionary(field => field.Name);
    public static void Patch(T component, T other, HashSet<string> targets)
    {
        foreach (var target in targets)
        {
            if (propertyDict.ContainsKey(target))
            {
                var property = propertyDict[target];
                if (!property.CanWrite)
                {
                    Log.LogError("ComponentPatcher", "Property is read only");
                    return;
                }
                property.SetValue(component, property.GetValue(other, null), null);
            }
            else if (fieldDict.ContainsKey(target))
            {
                var field = fieldDict[target];
                field.SetValue(component, field.GetValue(other));
            }
            else
            {
                Log.LogError("ComponentPatcher", "Property or field not found");
            }
        }
    }
}
