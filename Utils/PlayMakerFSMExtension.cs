using HutongGames.PlayMaker;
using System.Linq;

namespace DreamEchoesCore.Submodules.RingLib.Utils;

internal static class PlayMakerFSMExtension
{
    public static FsmState GetState(this PlayMakerFSM fsm, string name)
    {
        return fsm.FsmStates.Where(state => state.Name == name).First();
    }

    public static void RemoveAction(this FsmState state, int index)
    {
        var actions = state.Actions.ToList();
        actions.RemoveAt(index);
        state.Actions = actions.ToArray();
    }
}
