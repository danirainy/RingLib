using UnityEngine;

namespace RingLib.StateMachine;

internal class Transition { }

internal class StateTransition : Transition
{
    public static StateTransition operator +(StateTransition a, StateTransition b)
    {
        return a is ToState ? a : b;
    }
}

internal class CurrentState : StateTransition { }

internal class ToState : StateTransition
{
    public Type State;
}

internal class CoroutineTransition : Transition
{
    public IEnumerator<Transition>[] Routines;
    public IEnumerator<Transition> Routine
    {
        set
        {
            Routines = [value];
        }
    }
}

internal class WaitFor : CoroutineTransition
{
    public float Seconds
    {
        set
        {
            IEnumerator<Transition> routine()
            {
                var timer = value;
                while (timer > 0)
                {
                    timer -= Time.deltaTime;
                    yield return new CurrentState();
                }
            }
            Routines = [routine()];
        }
    }
}

internal class WaitTill : CoroutineTransition
{
    public Func<bool> Condition
    {
        set
        {
            IEnumerator<Transition> routine()
            {
                while (!value())
                {
                    yield return new CurrentState();
                }
            }
            Routines = [routine()];
        }
    }
}
