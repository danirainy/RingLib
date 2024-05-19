using HutongGames.PlayMaker.Actions;
using RingLib.Utils;
using System.Collections.Generic;
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

    private static GameObject stunEffectPrefab;

    static EntityStateMachine()
    {
        var greyPrince = Mod.GetPreloaded("GG_Grey_Prince_Zote", "Grey Prince");
        var fsm = greyPrince.LocateMyFSM("Control");
        var stunStart = fsm.GetState("Stun Start");
        var action = stunStart.GetAction<SpawnObjectFromGlobalPool>(7);
        stunEffectPrefab = action.gameObject.Value;
    }

    public EntityStateMachine(string startState, Dictionary<GlobalEvent, string> globalTransitions, bool spriteFacingLeft)
        : base(startState, globalTransitions)
    {
        this.spriteFacingLeft = spriteFacingLeft;
    }

    protected virtual void EntityStateMachineStart() { }

    protected sealed override void StateMachineStart()
    {
        BoxCollider2D = gameObject.GetComponent<BoxCollider2D>();
        Rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
        EntityStateMachineStart();
    }

    protected virtual void EntityStateMachineUpdate() { }

    protected sealed override void StateMachineUpdate()
    {
        EntityStateMachineUpdate();
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
            var raycastHit2D = Physics2D.Raycast(bottomRays[k], -Vector2.up, 0.05f, 1 << 8);
            if (raycastHit2D.collider != null)
            {
                return true;
            }
        }
        return false;
    }

    public virtual void OnHit() { }

    public virtual void OnDeath() { }

    protected void PlayStunEffect()
    {
        var stunEffect = Instantiate(stunEffectPrefab, Position, Quaternion.identity);
        stunEffect.SetActive(true);
    }
}
