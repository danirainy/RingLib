using UnityEngine;

namespace RingLib.StateMachine;

internal class EntityStateMachine : StateMachine
{
    public Vector3 Position
    {
        get
        {
            return gameObject.transform.position;
        }
        set
        {
            gameObject.transform.position = value;
        }
    }
    public BoxCollider2D BoxCollider2D;
    public Rigidbody2D Rigidbody2D;
    public Vector2 Velocity
    {
        get
        {
            return Rigidbody2D.velocity;
        }
        set
        {
            Rigidbody2D.velocity = value;
        }
    }
    private bool spriteFacingLeft;

    public EntityStateMachine(Type startState, Dictionary<string, Type> globalTransitions, bool spriteFacingLeft)
        : base(startState, globalTransitions)
    {
        this.spriteFacingLeft = spriteFacingLeft;
    }

    protected virtual void EnemyStateMachineStart() { }

    protected sealed override void StateMachineStart()
    {
        BoxCollider2D = gameObject.GetComponent<BoxCollider2D>();
        Rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        EnemyStateMachineStart();
    }

    protected virtual void EnemyStateMachineUpdate() { }

    protected sealed override void StateMachineUpdate()
    {
        EnemyStateMachineUpdate();
    }

    public float Direction()
    {
        var direction = Mathf.Sign(gameObject.transform.localScale.x);
        return spriteFacingLeft ? -direction : direction;
    }

    public bool Landed()
    {
        if (Rigidbody2D.velocity.y > 0)
        {
            return false;
        }
        var bottomRays = new List<Vector2>
        {
            BoxCollider2D.bounds.min,
            new Vector2(BoxCollider2D.bounds.center.x, BoxCollider2D.bounds.min.y),
            new Vector2(BoxCollider2D.bounds.max.x, BoxCollider2D.bounds.min.y)
        };
        for (var k = 0; k < 3; k++)
        {
            RaycastHit2D raycastHit2D3 = Physics2D.Raycast(bottomRays[k], -Vector2.up, 0.05f, 1 << 8);
            if (raycastHit2D3.collider != null)
            {
                return true;
            }
        }
        return false;
    }
}
