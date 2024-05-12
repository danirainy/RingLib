namespace RingLib.StateMachine;

internal class Transition { }

internal class CurrentState : Transition { }

internal class ToState : Transition
{
    public Type State;
}

internal class WaitFor : Transition
{
    public float Seconds;
}

internal class WaitTill : Transition
{
    public Func<bool> Condition;
}
