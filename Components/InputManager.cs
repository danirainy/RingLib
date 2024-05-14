using UnityEngine;

namespace RingLib.Components;

internal class InputManager : MonoBehaviour
{
    public HeroActions HeroActions;

    public bool LeftPressed;
    private bool leftPressed;
    public bool RightPressed;
    private bool rightPressed;
    public float Direction => LeftPressed ? -1 : RightPressed ? 1 : 0;

    public bool AttackPressed;
    private bool attackPressed;

    void Update()
    {
        if (leftPressed != HeroActions.left.IsPressed)
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
            leftPressed = HeroActions.left.IsPressed;
        }
        if (rightPressed != HeroActions.right.IsPressed)
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
            rightPressed = HeroActions.right.IsPressed;
        }
        if (attackPressed != HeroActions.attack.IsPressed)
        {
            AttackPressed = attackPressed;
            attackPressed = HeroActions.attack.IsPressed;
        }
    }
}
