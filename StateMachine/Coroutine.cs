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
        List<StateTransition> childrenTransition = children.Select(child => child.Update()).ToList();
        if (childrenTransition.Any(transition => transition == null))
        {
            children = [];
            return null;
        }
        return childrenTransition.Aggregate((a, b) => a + b);
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
            if (transition is CoroutineTransition composition)
            {
                children = composition.Routines.Select(routine => new Coroutine(routine)).ToList();
                continue;
            }
            Log.LogError(GetType().Name, $"Invalid transition {transition}");
            return null;
        }
    }
}
