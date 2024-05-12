namespace RingLib;

internal class Hooks
{
    public static void Initialize()
    {
        On.HeroController.RegainControl += HeroControllerRegainControl;
    }
    private static void HeroControllerRegainControl(On.HeroController.orig_RegainControl orig, HeroController self)
    {
        var control = self.gameObject.GetComponent<Control>();
        if (control != null && control.HasControlled)
        {
            return;
        }
        orig(self);
    }
}
