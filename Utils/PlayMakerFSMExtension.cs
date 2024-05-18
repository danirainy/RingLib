using HutongGames.PlayMaker;
using System;
using System.Linq;

namespace RingLib.Utils;

internal static class PlayMakerFSMExtension
{
    internal class CustomAction : FsmStateAction
    {
        private Action action;
        public CustomAction(Action action)
        {
            this.action = action;
        }
        public override void OnEnter()
        {
            action();
        }
    }

    public static FsmState GetState(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmStates.Where(state => state.Name == name).First();
    }

    public static T GetAction<T>(this FsmState state, int index) where T : FsmStateAction
    {
        return state.Actions[index] as T;
    }

    public static void AddCustomAction(this FsmState state, Action action)
    {
        var actions = state.Actions.ToList();
        actions.Add(new CustomAction(action));
        state.Actions = actions.ToArray();
    }

    public static void RemoveAction<T>(this FsmState state, int index) where T : FsmStateAction
    {
        var actions = state.Actions.ToList();
        if (actions[index] is not T)
        {
            Log.LogError(typeof(PlayMakerFSMExtension).Name, $"Action at index {index} is not of type {typeof(T).Name}");
        }
        actions.RemoveAt(index);
        state.Actions = actions.ToArray();
    }
}
