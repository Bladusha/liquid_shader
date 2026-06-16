using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class InputSystemCompat
{
    public static bool GetKeyDown(KeyCode keyCode)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        ButtonControl key = ResolveKey(keyboard, keyCode);
        return key != null && key.wasPressedThisFrame;
    }

    public static bool GetKey(KeyCode keyCode)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        ButtonControl key = ResolveKey(keyboard, keyCode);
        return key != null && key.isPressed;
    }

    public static float GetAxis(string axisName)
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        switch (axisName)
        {
            case "Horizontal":
                return GetButtonValue(keyboard, Key.D, Key.RightArrow) - GetButtonValue(keyboard, Key.A, Key.LeftArrow);
            case "Vertical":
                return GetButtonValue(keyboard, Key.W, Key.UpArrow) - GetButtonValue(keyboard, Key.S, Key.DownArrow);
            case "Mouse X":
                return mouse != null ? mouse.delta.ReadValue().x : 0f;
            case "Mouse Y":
                return mouse != null ? mouse.delta.ReadValue().y : 0f;
            default:
                return 0f;
        }
    }

    public static bool GetMouseButton(int button)
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return false;
        }

        switch (button)
        {
            case 0:
                return mouse.leftButton.isPressed;
            case 1:
                return mouse.rightButton.isPressed;
            case 2:
                return mouse.middleButton.isPressed;
            default:
                return false;
        }
    }

    private static float GetButtonValue(Keyboard keyboard, Key primary, Key secondary)
    {
        if (keyboard == null)
        {
            return 0f;
        }

        float value = 0f;
        if (keyboard[primary].isPressed)
        {
            value += 1f;
        }

        if (keyboard[secondary].isPressed)
        {
            value += 1f;
        }

        return Mathf.Clamp01(value);
    }

    private static ButtonControl ResolveKey(Keyboard keyboard, KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.C:
                return keyboard.cKey;
            case KeyCode.E:
                return keyboard.eKey;
            case KeyCode.F:
                return keyboard.fKey;
            case KeyCode.T:
                return keyboard.tKey;
            case KeyCode.K:
                return keyboard.kKey;
            case KeyCode.Tab:
                return keyboard.tabKey;
            case KeyCode.Space:
                return keyboard.spaceKey;
            case KeyCode.LeftShift:
                return keyboard.leftShiftKey;
            case KeyCode.LeftControl:
                return keyboard.leftCtrlKey;
            case KeyCode.Escape:
                return keyboard.escapeKey;
            case KeyCode.Return:
                return keyboard.enterKey;
            case KeyCode.KeypadEnter:
                return keyboard.numpadEnterKey;
            default:
                return null;
        }
    }
}
