using System;
using System.Collections.Generic;
using UnityEngine;

namespace RingLib.StateMachine;

internal class StateMachine : MonoBehaviour
{
    private static List<StateMachine> instances = [];
    private Dictionary<string, StateBase> states;
    public Type StartState;
    public string CurrentState { get; private set; }
    private Dictionary<string, string> globalTransitions = [];
    private Coroutine coroutine;

    public StateMachine(Type startState, Dictionary<string, Type> globalTransitions)
    {
        instances.Add(this);
        states = StateManager.GetStates(GetType());
        foreach (var state in states.Values)
        {
            state.StateMachine = this;
        }
        StartState = startState;
        foreach (var globalTransition in globalTransitions)
        {
            this.globalTransitions.Add(globalTransition.Key, globalTransition.Value.Name);
        }
    }

    private string CoroutineUpdate()
    {
        var transition = coroutine.Update();
        if (transition is NoTransition)
        {
            return null;
        }
        else if (transition is ToState toState)
        {
            return toState.State.Name;
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
        Log.LogInfo(GetType().Name, $"Entering state {state.GetType().Name}");
        coroutine = new Coroutine(state.Routine());
        return CoroutineUpdate();
    }

    private void ExitCurrentState(bool interrupted)
    {
        var state = states[CurrentState];
        Log.LogInfo(GetType().Name, $"Exiting state {state.GetType().Name}");
        coroutine = null;
        if (interrupted)
        {
            state.Interrupt();
        }
    }

    private void SetState(string state, bool interrupted)
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
                ExitCurrentState(interrupted);
            }
            CurrentState = state;
            state = EnterCurrentState();
            interrupted = false;
        }
    }

    public void SetState(Type State)
    {
        if (CurrentState == null)
        {
            Log.LogError(GetType().Name, "The state machine hasn't started yet");
            return;
        }
        SetState(State.Name, true);
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
            SetState(StartState.Name, false);
        }
        else
        {
            var nextState = CoroutineUpdate();
            if (nextState != null)
            {
                SetState(nextState, false);
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
        SetState(globalTransitions[event_], true);
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
