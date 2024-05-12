# Introduction

RingLib provides a lightweight state machine implementation ideal for developing mods, particularly modded bosses and controlled characters, in the game Hollow Knight. The typical use-case involves creating a custom StateMachine by inheriting from the RingLib state machine interface. Users also create custom states by inheriting from RingLib states. Developers would override member functions for each state to implement necessary actions and transitions.

# Example State
```csharp
// This example shows how to create a slightly complicated jump move with ease
internal class EvadeJump : State<MyStateMachine>
{
    public override Transition Enter()
    {
        StartCoroutine(Routine());
        return new CurrentState();
    }

    private IEnumerator<Transition> Routine()
    {
        // JumpStart
        var jumpRadiusMin = StateMachine.Config.EvadeJumpRadiusMin;
        var jumpRadiusMax = StateMachine.Config.EvadeJumpRadiusMax;
        var jumpRadius = UnityEngine.Random.Range(jumpRadiusMin, jumpRadiusMax);
        var targetXLeft = StateMachine.Target().Position().x - jumpRadius;
        var targetXRight = StateMachine.Target().Position().x + jumpRadius;
        float targetX;
        if (Mathf.Abs(StateMachine.Position.x - targetXLeft) < Mathf.Abs(StateMachine.Position.x - targetXRight))
        {
            targetX = targetXRight;
        }
        else
        {
            targetX = targetXLeft;
        }
        var velocityX = (targetX - StateMachine.Position.x) * StateMachine.Config.EvadeJumpVelocityXScale;
        if (Mathf.Sign(velocityX) != StateMachine.Direction())
        {
            StateMachine.Turn();
        }
        StateMachine.Animator.PlayAnimation("JumpStart");
        yield return new WaitTill { Condition = () => StateMachine.Animator.Finished };

        // JumpAscend
        StateMachine.Velocity = new Vector2(velocityX, StateMachine.Config.EvadeJumpVelocityY);
        StateMachine.Animator.PlayAnimation("JumpAscend");
        yield return new WaitTill { Condition = () => StateMachine.Velocity.y <= 0 };

        // JumpDescend
        StateMachine.Animator.PlayAnimation("JumpDescend");
        yield return new WaitTill { Condition = () => StateMachine.Landed() };

        // JumpEnd
        StateMachine.Velocity = Vector2.zero;
        StateMachine.Animator.PlayAnimation("JumpEnd");
        yield return new WaitTill { Condition = () => StateMachine.Animator.Finished };
        yield return new ToState { State = typeof(Attack) };
    }
}
```

# Example StateMachine
```csharp
// This example shows the inherited StateMachine that is used by the above state.
internal class MyStateMachine : EntityStateMachine
{
    public RingLib.Animator Animator { get; private set; }
    public RingLib.InputManager InputManager;
    public SeerStateMachine() : base(typeof(Idle), [])
    {
        SpriteFacingLeft = true; // If using EntityStateMachine.Direction
    }
    protected override void EnemyStateMachineStart()
    {
        // A separate GameObject for animation is good for adjusting offsets
        var animation = gameObject.transform.Find("Animation");
        Animator = animation.GetComponent<RingLib.Animator>();
        // Accepts input for a controlled character
        InputManager = gameObject.AddComponent<RingLib.InputManager>();
    }
    protected override void EnemyStateMachineUpdate() {}
}
```
