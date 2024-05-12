using HKMirror.Reflection;
using UnityEngine;

namespace RingLib;

internal class InputManager : MonoBehaviour
{
    public bool LeftPressed;
    private bool leftPressed;
    public bool RightPressed;
    private bool rightPressed;
    public float Direction => LeftPressed ? -1 : RightPressed ? 1 : 0;
    public bool AttackPressed;
    private bool attackPressed;
    private HeroActions heroActions;
    public InputManager()
    {
        heroActions = HeroController.instance.Reflect().inputHandler.inputActions;
    }
    void Update()
    {
        if (leftPressed != heroActions.left.IsPressed)
        {
            if (!leftPressed)
            {
                LeftPressed = true;
                RightPressed = false;
            }
            else
            {
                LeftPressed = false;
                RightPressed = rightPressed;
            }
            leftPressed = heroActions.left.IsPressed;
        }
        if (rightPressed != heroActions.right.IsPressed)
        {
            if (!rightPressed)
            {
                RightPressed = true;
                LeftPressed = false;
            }
            else
            {
                RightPressed = false;
                LeftPressed = leftPressed;
            }
            rightPressed = heroActions.right.IsPressed;
        }
        if (attackPressed != heroActions.attack.IsPressed)
        {
            AttackPressed = attackPressed;
            attackPressed = heroActions.attack.IsPressed;
        }
    }
}
