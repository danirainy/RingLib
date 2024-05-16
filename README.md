# Introduction

RingLib provides a lightweight state machine implementation ideal for developing mods, particularly modded bosses and controlled characters, in the game Hollow Knight. The typical use-case involves creating a custom StateMachine by inheriting from the RingLib StateMachine or its specialized subclasses. Users can create a state by adding a member function with type `Func<IEnumerator<Transition>>` to the state and marking it with `[State]`.

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
internal partial class MyStateMachine : EntityStateMachine
{
    [State]
    private IEnumerator<Transition> EvadeJump()
    {
        // JumpStart
        var jumpRadiusMin = Config.EvadeJumpRadiusMin;
        var jumpRadiusMax = Config.EvadeJumpRadiusMax;
        var jumpRadius = UnityEngine.Random.Range(jumpRadiusMin, jumpRadiusMax);
        var targetXLeft = Target().Position().x - jumpRadius;
        var targetXRight = Target().Position().x + jumpRadius;
        float targetX;
        if (Mathf.Abs(Position.x - targetXLeft) < Mathf.Abs(Position.x - targetXRight))
        {
            targetX = targetXRight;
        }
        else
        {
            targetX = targetXLeft;
        }
        var velocityX = (targetX - Position.x) * Config.EvadeJumpVelocityXScale;
        if (Mathf.Sign(velocityX) != Direction())
        {
            yield return new CoroutineTransition { Routine = Turn() };
        }
        yield return new CoroutineTransition { Routine = Animator.PlayAnimation("JumpStart") };

        // JumpAscend
        Velocity = new Vector2(velocityX, Config.EvadeJumpVelocityY);
        Animator.PlayAnimation("JumpAscend");
        yield return new WaitTill { Condition = () => Velocity.y <= 0 };

        // JumpDescend
        Animator.PlayAnimation("JumpDescend");
        yield return new WaitTill { Condition = () => Landed() };

        // JumpEnd
        Velocity = Vector2.zero;
        yield return new CoroutineTransition { Routine = Animator.PlayAnimation("JumpEnd") };
        yield return new ToState { State = nameof(Attack) };
    }
}
```

# Example Triple Slash State
```csharp
internal partial class MyStateMachine : EntityStateMachine
{
    [State]
    private IEnumerator<Transition> Slash()
    {
        if (!FacingTarget())
        {
            yield return new CoroutineTransition
            {
                Routine = Turn()
            };
        }

        var velocityX = (Target().Position().x - Position.x);
        velocityX *= Config.SlashVelocityXScale;
        var minVelocityX = Config.ControlledSlashVelocityX;
        if (velocityX > -minVelocityX && velocityX < minVelocityX)
        {
            velocityX = Mathf.Sign(velocityX) * minVelocityX;
        }
        Velocity = Vector2.zero;

        IEnumerator<Transition> Slash(string slash)
        {
            if (!FacingTarget())
            {
                velocityX *= -1;
                Velocity *= -1;
                yield return new CoroutineTransition
                {
                    Routine = Turn()
                };
            }
            var previousVelocityX = Velocity.x;
            Transition updater(float normalizedTime)
            {
                var currentVelocityX = Mathf.Lerp(previousVelocityX, velocityX, normalizedTime);
                Velocity = new Vector2(currentVelocityX, 0);
                return new NoTransition();
            }
            yield return new CoroutineTransition
            {
                Routine = Animator.PlayAnimation(slash, updater)
            };
        }
        foreach (var slash in new string[] { "Slash1", "Slash2", "Slash3" })
        {
            yield return new CoroutineTransition
            {
                Routine = Slash(slash)
            };
        }

        yield return new ToState { State = nameof(Idle) };
    }
}
```

# Example StateMachine
```csharp
internal class MyStateMachine : EntityStateMachine
{
    public RingLib.Animator Animator { get; private set; }
    public RingLib.InputManager InputManager { get; private set; }

    public MyStateMachine() : base(nameof(Idle), [], /*SpriteFacingLeft =*/true) {}

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

    public GameObject Target()
    {
        return HeroController.instance.gameObject;
    }

    public bool FacingTarget()
    {
        return Mathf.Sign(Target().transform.position.x - transform.position.x) == Direction();
    }

    public IEnumerator<Transition> Turn()
    {
        var localScale = gameObject.transform.localScale;
        localScale.x *= -1;
        gameObject.transform.localScale = localScale;
        yield return new NoTransition();
    }
}
```
