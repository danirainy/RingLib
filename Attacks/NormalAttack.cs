namespace RingLib.Attacks;

internal class NormalAttack : UnparryableNailSlash
{
    private new void Start()
    {
        base.Start();

        Destroy(damageHero.LocateMyFSM("nail_clash_tink"));
    }
}
