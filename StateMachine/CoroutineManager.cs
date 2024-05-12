namespace RingLib.StateMachine;

internal class CoroutineManager
{
    private List<Coroutine> activeCoroutines = new();
    public void StartCoroutine(IEnumerator<Transition> coroutine)
    {
        activeCoroutines.Add(new Coroutine(coroutine));
    }
    public Transition UpdateCoroutines()
    {
        var newActiveCoroutines = new List<Coroutine>();
        foreach (var coroutine in activeCoroutines)
        {
            var transition = coroutine.Update();
            if (transition != null)
            {
                if (transition is CurrentState)
                {
                    newActiveCoroutines.Add(coroutine);
                }
                else if (transition is ToState)
                {
                    return transition;
                }
                else
                {
                    Log.LogError(GetType().Name, $"Invalid transition {transition}");
                }
            }
        }
        activeCoroutines = newActiveCoroutines;
        return new CurrentState();
    }
    public void StopCoroutines()
    {
        activeCoroutines.Clear();
    }
}
