using UnityEngine;

namespace RingLib.StateMachine;

internal static class GameObjectExtension
{
    public static Vector3 Position(this GameObject gameObject)
    {
        return gameObject.transform.position;
    }

    public static void BroadcastEventInChildren(this GameObject gameObject, Event event_)
    {
        foreach (var stateMachine in gameObject.GetComponentsInChildren<StateMachine>())
        {
            stateMachine.ReceiveEvent(event_);
        }
    }

    public static void BroadcastEventInParent(this GameObject gameObject, Event event_)
    {
        while (gameObject.transform.parent != null)
        {
            gameObject = gameObject.transform.parent.gameObject;
        }
        gameObject.BroadcastEventInChildren(event_);
    }
}
