using System.Collections.Generic;

namespace RingLib.StateMachine;

internal class StateBase
{
    public StateMachine StateMachine { get; set; }

    public virtual IEnumerator<Transition> Routine() { yield break; }

    public virtual void Interrupt() { }
}
internal class State<TStateMachine> : StateBase where TStateMachine : StateMachine
{
    public new TStateMachine StateMachine
    {
        get => base.StateMachine as TStateMachine;
    }
}
