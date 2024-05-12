using System.Reflection;

namespace RingLib.StateMachine;

internal class StateManager
{
    static Dictionary<Type, List<Type>> statesTypes;
    private static void CollectStates()
    {
        statesTypes = new();
        var stateMachineTypes = Assembly.GetExecutingAssembly().GetTypes().
            Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(StateMachine)));
        foreach (var stateMachineType in stateMachineTypes)
        {
            Log.LogInfo("StateManager", $"Collecting states for {stateMachineType.Name}");
            var stateGenericType = typeof(State<>).MakeGenericType(stateMachineType);
            var stateTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(stateGenericType)).ToList();
            foreach (var stateType in stateTypes)
            {
                Log.LogInfo("StateManager", $"    Collected state {stateType.Name}");
            }
            statesTypes.Add(stateMachineType, stateTypes);
        }
    }
    public static Dictionary<string, StateBase> GetStates(Type type)
    {
        if (statesTypes == null)
        {
            CollectStates();
        }
        return statesTypes[type].Select(t => (StateBase)Activator.CreateInstance(t)).
            ToDictionary(s => s.GetType().Name, s => s);
    }
}
