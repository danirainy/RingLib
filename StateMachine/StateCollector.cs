using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RingLib.StateMachine;

using State = Func<IEnumerator<Transition>>;

internal class StateCollector
{
    private static Dictionary<Type, Dictionary<string, MethodInfo>> states;

    private static void CollectStates()
    {
        states = [];
        var stateMachineTypes = Assembly.GetExecutingAssembly().GetTypes().
            Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(StateMachine)));
        foreach (var stateMachineType in stateMachineTypes)
        {
            Log.LogInfo(typeof(StateCollector).Name, $"Collecting states for {stateMachineType.Name}");
            Dictionary<string, MethodInfo> statesPerStateMachine = [];
            var bingingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var methods = stateMachineType.GetMethods(bingingFlags);
            foreach (var method in methods)
            {
                if (method.GetCustomAttributes(typeof(StateAttribute), false).Length > 0)
                {
                    Log.LogInfo(typeof(StateCollector).Name, $"    Collected state {method.Name}");
                    statesPerStateMachine.Add(method.Name, method);
                }
            }
            states.Add(stateMachineType, statesPerStateMachine);
        }
    }

    public static Dictionary<string, State> GetStates(StateMachine stateMachine)
    {
        if (states == null)
        {
            CollectStates();
        }
        var type = stateMachine.GetType();
        Dictionary<string, State> statesPerStateMachine = [];
        foreach (var state in states[type])
        {
            var stateDelegate =
                Delegate.CreateDelegate(typeof(State), stateMachine, state.Value);
            statesPerStateMachine.Add(state.Key, (State)stateDelegate);
        }
        return statesPerStateMachine;
    }
}
