using UnityEngine;

namespace RingLib.Attack;

internal class NailSlash : Attack
{
    public int DamageHero;
    public int DamageEnemy;
    private GameObject damageHero;
    private GameObject damageEnemy;
    private GameObject damageEnemyTinker;
    private Collider2D originalCollider;

    private void Start()
    {
        var damageHeroPrefab = Preloader.Get("GG_Sly/Battle Scene/Sly Boss/S1");
        damageHero = Instantiate(damageHeroPrefab);
        damageHero.name = "DamageHero";
        damageHero.transform.parent = transform;
        damageHero.transform.localPosition = Vector3.zero;
        damageHero.transform.localScale = Vector3.one;
        Destroy(damageHero.GetComponent<Collider2D>());
        damageHero.GetComponent<DamageHero>().damageDealt = DamageHero;
        damageHero.SetActive(true);

        damageEnemy = new GameObject("DamageEnemy");
        damageEnemy.layer = LayerMask.NameToLayer("Attack");
        damageEnemy.transform.parent = transform;
        damageEnemy.transform.localPosition = Vector3.zero;
        damageEnemy.transform.localScale = Vector3.one;
        var damageEnemies = damageEnemy.AddComponent<DamageEnemies>();
        var damageEnemiesSlash = HeroController.instance.transform.Find("Attacks").Find("Slash").gameObject.LocateMyFSM("damages_enemy");
        damageEnemies.attackType = (AttackTypes)damageEnemiesSlash.FsmVariables.GetFsmInt("attackType").Value;
        damageEnemies.circleDirection = damageEnemiesSlash.FsmVariables.GetFsmBool("circleDirection").Value;
        damageEnemies.damageDealt = DamageEnemy;
        damageEnemies.direction = damageEnemiesSlash.FsmVariables.GetFsmFloat("direction").Value;
        damageEnemies.ignoreInvuln = damageEnemiesSlash.FsmVariables.GetFsmBool("Ignore Invuln").Value;
        damageEnemies.magnitudeMult = damageEnemiesSlash.FsmVariables.GetFsmFloat("magnitudeMult").Value;
        damageEnemies.moveDirection = damageEnemiesSlash.FsmVariables.GetFsmBool("moveDirection").Value;
        damageEnemies.specialType = (SpecialTypes)damageEnemiesSlash.FsmVariables.GetFsmInt("Special Type").Value;
        damageEnemy.SetActive(false);

        damageEnemyTinker = new GameObject("DamageEnemyTinker");
        damageEnemyTinker.layer = LayerMask.NameToLayer("Tinker");
        damageEnemyTinker.transform.parent = transform;
        damageEnemyTinker.transform.localPosition = Vector3.zero;
        damageEnemyTinker.transform.localScale = Vector3.one;
        damageEnemyTinker.SetActive(false);

        originalCollider = GetComponent<Collider2D>();
    }
    private void CopyCollider(BoxCollider2D original, BoxCollider2D copy)
    {
        ComponentPatcher<BoxCollider2D>.Patch(copy, original, ["isTrigger", "offset", "size"]);
    }
    private void CopyCollider(PolygonCollider2D original, PolygonCollider2D copy)
    {
        ComponentPatcher<PolygonCollider2D>.Patch(copy, original, ["isTrigger", "points"]);
    }
    private void InstallColliderIfNotExist(GameObject gameObject)
    {
        var originalColliderType = originalCollider.GetType();
        if (gameObject.GetComponent(originalColliderType) == null)
        {
            var collider = gameObject.AddComponent(originalColliderType);
            if (collider is BoxCollider2D boxCollider2D)
            {
                CopyCollider(originalCollider as BoxCollider2D, boxCollider2D);
            }
            else if (collider is PolygonCollider2D polygonCollider2D)
            {
                CopyCollider(originalCollider as PolygonCollider2D, polygonCollider2D);
            }
            else
            {
                Log.LogError(GetType().Name, "Unknown collider type");
            }
        }
    }
    private void Update()
    {
        InstallColliderIfNotExist(damageHero);
        InstallColliderIfNotExist(damageEnemy);
        InstallColliderIfNotExist(damageEnemyTinker);
        if (Hero)
        {
            damageHero.SetActive(false);
            damageEnemy.SetActive(true);
            damageEnemyTinker.SetActive(true);
        }
        else
        {
            damageHero.SetActive(true);
            damageEnemy.SetActive(false);
            damageEnemyTinker.SetActive(false);
        }
    }
}
