using UnityEngine;

namespace RingLib;

internal class Control : MonoBehaviour
{
    public bool HasControlled { get; private set; }
    public GameObject Controlled { get; private set; }
    private float originalGravityScale;
    private Vector2 originalColliderOffset;
    private Vector2 originalColliderSize;
    private Vector2 originalHeroBoxColliderOffset;
    private Vector2 originalHeroBoxColliderSize;
    private void DestroyControlled()
    {
        if (!HasControlled)
        {
            Log.LogError(GetType().Name, "No Controlled to destroy");
            return;
        }
        GameObject.Destroy(Controlled);
    }
    public void InstallControlled(GameObject gameObject)
    {
        if (HasControlled)
        {
            DestroyControlled();
        }
        else
        {
            var heroController = HeroController.instance;
            if (heroController.controlReqlinquished)
            {
                Log.LogError(GetType().Name, "Hero control was relinquished");
                return;
            }
            heroController.RelinquishControl();
            originalGravityScale = GetComponent<Rigidbody2D>().gravityScale;
            GetComponent<Rigidbody2D>().gravityScale = 0;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Terrain"));
            GetComponent<tk2dSprite>().color = Vector4.zero;
            var heroCollider = GetComponent<BoxCollider2D>();
            originalColliderOffset = heroCollider.offset;
            originalColliderSize = heroCollider.size;
            var heroBoxCollider = transform.Find("HeroBox").GetComponent<BoxCollider2D>();
            originalHeroBoxColliderOffset = heroBoxCollider.offset;
            originalHeroBoxColliderSize = heroBoxCollider.size;
            HasControlled = true;
        }
        Controlled = gameObject;
    }
    public void UninstallControlled()
    {
        if (!HasControlled)
        {
            Log.LogError(GetType().Name, "No Controlled to uninstall");
            return;
        }
        DestroyControlled();
        HasControlled = false;
        var heroController = HeroController.instance;
        heroController.RegainControl();
        GetComponent<Rigidbody2D>().gravityScale = originalGravityScale;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Terrain"), false);
        GetComponent<tk2dSprite>().color = Vector4.one;
        var heroCollider = GetComponent<BoxCollider2D>();
        heroCollider.offset = originalColliderOffset;
        heroCollider.size = originalColliderSize;
        var heroBoxCollider = transform.Find("HeroBox").GetComponent<BoxCollider2D>();
        heroBoxCollider.offset = originalHeroBoxColliderOffset;
        heroBoxCollider.size = originalHeroBoxColliderSize;
    }
    private void Update()
    {
        if (HasControlled)
        {
            if (Controlled == null)
            {
                UninstallControlled();
            }
            else
            {
                transform.position = Controlled.transform.position;
                var rigidbody2D = GetComponent<Rigidbody2D>();
                rigidbody2D.velocity = Vector2.zero;
                GetComponent<tk2dSprite>().color = Vector4.zero;
            }
        }
    }
}
