namespace RingLib.StateMachine;

internal class StateBase
{
    public StateMachine StateMachine { get; set; }
    public virtual Transition Enter() { return new CurrentState(); }
    public virtual void Exit(bool interrupted) { }
    public virtual Transition Update() { return new CurrentState(); }
    public void StartCoroutine(IEnumerator<Transition> routine)
    {
        if (StateMachine == null)
        {
            Log.LogError(GetType().Name, $"StateMachine is null");
            return;
        }
        StateMachine.StartCoroutine(routine);
    }
}
internal class State<TStateMachine> : StateBase where TStateMachine : StateMachine
{
    public new TStateMachine StateMachine
    {
        get => base.StateMachine as TStateMachine;
    }
}
