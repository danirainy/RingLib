using System.Reflection;
using UnityEngine;

namespace RingLib.Components;

internal class InputManager : MonoBehaviour
{
    private HeroActions heroActions;

    public bool LeftPressed;
    private bool leftPressed;
    public bool RightPressed;
    private bool rightPressed;
    public float Direction => LeftPressed ? -1 : RightPressed ? 1 : 0;

    public bool AttackPressed;
    private bool attackPressed;

    private void Start()
    {
        var heroControllerType = typeof(HeroController);
        var inputHandlerField = heroControllerType.GetField(
            "inputHandler", BindingFlags.NonPublic | BindingFlags.Instance);
        var inputHandler = inputHandlerField.GetValue(HeroController.instance) as InputHandler;
        heroActions = inputHandler.inputActions;
    }

    private void Update()
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
            attackPressed = heroActions.attack.IsPressed;
            AttackPressed = attackPressed;
        }
    }
}
