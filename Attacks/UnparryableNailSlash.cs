using DreamEchoesCore.Submodules.RingLib.Utils;

namespace RingLib.Attacks;

internal class UnparryableNailSlash : NailSlash
{
    private new void Start()
    {
        base.Start();
        var fsm = damageHero.LocateMyFSM("nail_clash_tink");
        var state = fsm.GetState("Blocked Hit");
        state.RemoveAction(1);
        state.RemoveAction(0);
    }
}
