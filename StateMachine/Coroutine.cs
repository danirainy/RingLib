using UnityEngine;
namespace RingLib.StateMachine;

internal class Coroutine
{
    private IEnumerator<Transition> enumerator;
    private float time;
    private Func<bool> condition;
    public Coroutine(IEnumerator<Transition> enumerator)
    {
        this.enumerator = enumerator;
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
        if (enumerator.MoveNext())
        {
            var transition = enumerator.Current;
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
