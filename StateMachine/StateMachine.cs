using System;
using System.Collections.Generic;
using UnityEngine;

namespace RingLib.StateMachine;

using State = Func<IEnumerator<Transition>>;

[AttributeUsage(AttributeTargets.Method)]
internal class StateAttribute : Attribute { }

internal class StateMachine : MonoBehaviour
{
    private static List<StateMachine> instances = [];
    private Dictionary<string, State> states;
    public string StartState;
    public string CurrentState { get; private set; }
    private Dictionary<string, string> globalTransitions = [];
    private Coroutine coroutine;

    public StateMachine(string startState, Dictionary<string, string> globalTransitions)
    {
        instances.Add(this);
        states = StateCollector.GetStates(this);
        StartState = startState;
        this.globalTransitions = globalTransitions;
    }

    private string CoroutineUpdate()
    {
        var transition = coroutine.Update();
        if (transition == null || transition is NoTransition)
        {
            return null;
        }
        else if (transition is ToState toState)
        {
            return toState.State;
        }
        else
        {
            Log.LogError(GetType().Name, $"Invalid transition type {transition.GetType().Name}");
            return null;
        }
    }

    private string EnterCurrentState()
    {
        var state = states[CurrentState];
        Log.LogInfo(GetType().Name, $"Entering state {CurrentState}");
        coroutine = new Coroutine(state());
        return CoroutineUpdate();
    }

    private void ExitCurrentState()
    {
        Log.LogInfo(GetType().Name, $"Exiting state {CurrentState}");
        coroutine = null;
    }

    private void SetStateInternal(string state)
    {
        while (state != null)
        {
            if (!states.ContainsKey(state))
            {
                Log.LogError(GetType().Name, $"Invalid state {state} to set to");
                return;
            }
            if (CurrentState != null)
            {
                ExitCurrentState();
            }
            CurrentState = state;
            state = EnterCurrentState();
        }
    }

    public void SetState(string state)
    {
        if (CurrentState == null)
        {
            Log.LogError(GetType().Name, "The state machine hasn't started yet");
            return;
        }
        SetStateInternal(state);
    }

    protected virtual void StateMachineStart() { }

    private void Start()
    {
        StateMachineStart();
    }

    protected virtual void StateMachineUpdate() { }

    private void Update()
    {
        if (CurrentState == null)
        {
            SetStateInternal(StartState);
        }
        else
        {
            var nextState = CoroutineUpdate();
            if (nextState != null)
            {
                SetStateInternal(nextState);
            }
        }
        StateMachineUpdate();
    }

    public void ReceiveEvent(string event_)
    {
        if (!globalTransitions.ContainsKey(event_))
        {
            Log.LogInfo(GetType().Name, $"No global transition for {event_}");
            return;
        }
        if (CurrentState == null)
        {
            Log.LogError(GetType().Name, "The state machine hasn't started yet");
            return;
        }
        SetStateInternal(globalTransitions[event_]);
    }

    public static List<StateMachine> GetInstances()
    {
        instances.RemoveAll(instance => instance == null);
        return instances;
    }

    public static void BroadcastEvent(string event_)
    {
        foreach (var instance in GetInstances())
        {
            instance.ReceiveEvent(event_);
        }
    }
}
