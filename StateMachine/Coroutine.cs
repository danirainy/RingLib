using System.Collections.Generic;
using System.Linq;

namespace RingLib.StateMachine;

internal class Coroutine
{
    private IEnumerator<Transition> routine;
    private List<Coroutine> children = [];

    public Coroutine(IEnumerator<Transition> routine)
    {
        this.routine = routine;
    }

    public StateTransition UpdateChildren()
    {
        if (children.Count == 0)
        {
            return null;
        }
        StateTransition stateTransition = new NoTransition();
        foreach (var childTransition in children.Select(child => child.Update()))
        {
            if (childTransition == null)
            {
                children = [];
                return null;
            }
            if (childTransition is ToState)
            {
                stateTransition = childTransition;
            }
        }
        return stateTransition;
    }

    public StateTransition Update()
    {
        while (true)
        {
            var childrenTransition = UpdateChildren();
            if (childrenTransition != null)
            {
                return childrenTransition;
            }
            if (!routine.MoveNext())
            {
                return null;
            }
            var transition = routine.Current;
            if (transition is StateTransition stateTransition)
            {
                return stateTransition;
            }
            if (transition is CoroutineTransition coroutineTransition)
            {
                children = coroutineTransition.RoutinesInternal.Select(routine => new Coroutine(routine)).ToList();
                continue;
            }
            Log.LogError(GetType().Name, $"Invalid transition {transition}");
            return null;
        }
    }
}
