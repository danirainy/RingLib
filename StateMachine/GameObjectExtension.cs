using UnityEngine;

namespace RingLib.StateMachine;

internal static class GameObjectExtension
{
    public static Vector3 Position(this GameObject gameObject)
    {
        return gameObject.transform.position;
    }
    public static void BroadcastEvent(this GameObject gameObject, string event_)
    {
        foreach (var stateMachine in gameObject.GetComponentsInChildren<StateMachine>())
        {
            stateMachine.ReceiveEvent(event_);
        }
    }
}
