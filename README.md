# Introduction

RingLib provides a lightweight state machine implementation ideal for developing mods, particularly modded bosses and controlled characters, in the game Hollow Knight. The typical use-case involves creating a custom StateMachine by inheriting from the RingLib state machine interface. Users also create custom states by inheriting from RingLib states and override member functions for each state to implement necessary actions and transitions.

# Feature List
| Feature                                                       | Supported           |
|---------------------------------------------------------------|---------------------|
| State Machine Abstraction for Enemies & Controlled Characters | Yes                 |
| In-State Coroutine Tree Support                               | Yes                 |
| State Machine Integration with Unity Animator                 | Yes                 |
| Various Dev Utils                                             | Yes                 |
| Unity Editor Template for Hollow Knight Modding               | No (Use WeaverCore) |

# Coroutine Tree

RingLib provides extensive support for in-state coroutines. All the state actions are expected to be done with coroutines instead of explicit Enter/Update/Exit functions. A coroutine is allowed to recursively call another coroutine. It is also allowed to spawn multiple coroutines at the same time at any level of the recursion. Parallel child coroutines will exit together if any of their siblings finish. This provides powerful building blocks for large and complex states.

# Example Evade Jump State
```csharp
internal class EvadeJump : State<MyStateMachine>
{
    public override IEnumerator<Transition> Routine()
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
            yield return new CoroutineTransition { Routine = StateMachine.Turn() };
        }
        yield return new CoroutineTransition { Routine = StateMachine.Animator.PlayAnimation("JumpStart") };

        // JumpAscend
        StateMachine.Velocity = new Vector2(velocityX, StateMachine.Config.EvadeJumpVelocityY);
        StateMachine.Animator.PlayAnimation("JumpAscend");
        yield return new WaitTill { Condition = () => StateMachine.Velocity.y <= 0 };

        // JumpDescend
        StateMachine.Animator.PlayAnimation("JumpDescend");
        yield return new WaitTill { Condition = () => StateMachine.Landed() };

        // JumpEnd
        StateMachine.Velocity = Vector2.zero;
        yield return new CoroutineTransition { Routine = StateMachine.Animator.PlayAnimation("JumpEnd") };
        yield return new ToState { State = typeof(Attack) };
    }
}
```

# Example Triple Slash State
```csharp
internal class Slash : State<MyStateMachine>
{
    public override IEnumerator<Transition> Routine()
    {
        if (!StateMachine.FacingTarget())
        {
            yield return new CoroutineTransition
            {
                Routine = StateMachine.Turn()
            };
        }

        var velocityX = (StateMachine.Target().Position().x - StateMachine.Position.x);
        velocityX *= StateMachine.Config.SlashVelocityXScale;
        var minVelocityX = StateMachine.Config.ControlledSlashVelocityX;
        if (velocityX > -minVelocityX && velocityX < minVelocityX)
        {
            velocityX = Mathf.Sign(velocityX) * minVelocityX;
        }
        StateMachine.Velocity = Vector2.zero;

        IEnumerator<Transition> Slash(string slash)
        {
            if (!StateMachine.FacingTarget())
            {
                velocityX *= -1;
                StateMachine.Velocity *= -1;
                yield return new CoroutineTransition
                {
                    Routine = StateMachine.Turn()
                };
            }
            var previousVelocityX = StateMachine.Velocity.x;
            Transition updater(float normalizedTime)
            {
                var currentVelocityX = Mathf.Lerp(previousVelocityX, velocityX, normalizedTime);
                StateMachine.Velocity = new Vector2(currentVelocityX, 0);
                return new NoTransition();
            }
            yield return new CoroutineTransition
            {
                Routine = StateMachine.Animator.PlayAnimation(slash, updater)
            };
        }
        foreach (var slash in new string[] { "Slash1", "Slash2", "Slash3" })
        {
            yield return new CoroutineTransition
            {
                Routine = Slash(slash)
            };
        }

        yield return new ToState { State = typeof(Idle) };
    }
}
```

# Example StateMachine
```csharp
internal class MyStateMachine : EntityStateMachine
{
    public RingLib.Animator Animator { get; private set; }
    public RingLib.InputManager InputManager { get; private set; }

    public MyStateMachine() : base(typeof(Idle), [], /*SpriteFacingLeft =*/true) {}

    protected override void EnemyStateMachineStart()
    {
        // A separate GameObject for animation is good for adjusting offsets
        var animation = gameObject.transform.Find("Animation");
        Animator = animation.GetComponent<RingLib.Animator>();
        // Accepts input for a controlled character
        InputManager = gameObject.AddComponent<RingLib.InputManager>();
        InputManager.HeroActions = HeroController.instance.Reflect().inputHandler.inputActions;
    }

    protected override void EnemyStateMachineUpdate() {}
}
```
