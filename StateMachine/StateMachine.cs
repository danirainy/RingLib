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
    private Coroutine coroutine;
    private HashSet<InStateEvent> inStateEvents = [];
    private Dictionary<GlobalEvent, string> globalTransitions = [];

    public StateMachine(string startState, Dictionary<GlobalEvent, string> globalTransitions)
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

    protected bool CheckInStateEvent(InStateEvent inStateEvent)
    {
        return inStateEvents.Contains(inStateEvent);
    }

    public void ReceiveEvent(Event event_)
    {
        if (event_ is InStateEvent inStateEvent)
        {
            inStateEvents.Add(inStateEvent);
        }
        else if (event_ is GlobalEvent globalEvent)
        {
            if (!globalTransitions.ContainsKey(globalEvent))
            {
                Log.LogInfo(GetType().Name, $"No global transition for {globalEvent}");
                return;
            }
            if (CurrentState == null)
            {
                Log.LogError(GetType().Name, "The state machine hasn't started yet");
                return;
            }
            SetStateInternal(globalTransitions[globalEvent]);
        }
        else
        {
            Log.LogError(GetType().Name, $"Invalid event type {event_.GetType().Name}");
        }
    }

    public static List<StateMachine> GetInstances()
    {
        instances.RemoveAll(instance => instance == null);
        return instances;
    }

    public static void BroadcastEvent(GlobalEvent event_)
    {
        foreach (var instance in GetInstances())
        {
            instance.ReceiveEvent(event_);
        }
    }
}
