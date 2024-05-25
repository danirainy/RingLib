using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RingLib.StateMachine;

using State = Func<IEnumerator<Transition>>;

[AttributeUsage(AttributeTargets.Method)]
internal class StateAttribute : Attribute { }

internal class Event { }

internal class StateMachine : MonoBehaviour
{
    private static List<StateMachine> instances = [];
    private Dictionary<string, State> states;
    public string StartState;
    public string CurrentState { get; private set; }
    private Coroutine coroutine;
    private HashSet<Event> inStateEvents = [];
    private Dictionary<Type, string> globalTransitions = [];

    public StateMachine(string startState, Dictionary<Type, string> globalTransitions)
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
        inStateEvents.Clear();
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

        inStateEvents.Clear();

        StateMachineUpdate();
    }

    protected List<Event> CheckInStateEvent<T>() where T : Event
    {
        return inStateEvents.Where(e => e.GetType() == typeof(T)).ToList();
    }

    public void ReceiveEvent(Event event_)
    {
        var type = event_.GetType();
        if (!globalTransitions.ContainsKey(type))
        {
            if (CurrentState == null)
            {
                Log.LogInfo(GetType().Name, "The state machine hasn't started yet");
            }
            else
            {
                inStateEvents.Add(event_);
            }
        }
        else
        {
            if (CurrentState == null)
            {
                Log.LogError(GetType().Name, "The state machine hasn't started yet");
            }
            else
            {
                SetStateInternal(globalTransitions[type]);
            }
        }
    }

    public static List<StateMachine> GetInstances()
    {
        instances.RemoveAll(instance => instance == null);
        return instances;
    }

    public static void BroadcastEvent(Event event_)
    {
        foreach (var instance in GetInstances())
        {
            instance.ReceiveEvent(event_);
        }
    }
}
