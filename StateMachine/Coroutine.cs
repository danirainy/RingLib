using UnityEngine;
namespace RingLib.StateMachine;

internal class Coroutine
{
    private IEnumerator<Transition> routine;
    private float time;
    private Func<bool> condition;
    public Coroutine(IEnumerator<Transition> routine)
    {
        this.routine = routine;
        time = 0;
    }
    public Transition Update()
    {
        if (time > 0)
        {
            time -= Time.deltaTime;
            if (time > 0)
            {
                return new CurrentState();
            }
        }
        if (condition != null && !condition())
        {
            return new CurrentState();
        }
        condition = null;
        if (routine.MoveNext())
        {
            var transition = routine.Current;
            if (transition is CurrentState || transition is ToState)
            {
                return transition;
            }
            else if (transition is WaitFor waitFor)
            {
                time = waitFor.Seconds;
                return new CurrentState();
            }
            else if (transition is WaitTill waitTill)
            {
                condition = waitTill.Condition;
                return new CurrentState();
            }
            else
            {
                Log.LogError(GetType().Name, $"Invalid transition {transition}");
            }
        }
        return null;
    }
}
