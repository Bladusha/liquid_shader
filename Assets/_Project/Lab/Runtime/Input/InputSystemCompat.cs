using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public static class InputSystemCompat
{
    public static bool GetKeyDown(KeyCode keyCode)
    {
        if (TryGetKeyFromInputSystem(keyCode, out ButtonControl key))
        {
            return key.wasPressedThisFrame;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.GetKeyDown(keyCode);
#else
        return false;
#endif
    }

    public static bool GetKey(KeyCode keyCode)
    {
        if (TryGetKeyFromInputSystem(keyCode, out ButtonControl key))
        {
            return key.isPressed;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.GetKey(keyCode);
#else
        return false;
#endif
    }

    public static float GetAxis(string axisName)
    {
        if (TryGetAxisFromInputSystem(axisName, out float value))
        {
            return value;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.GetAxis(axisName);
#else
        return 0f;
#endif
    }

    public static bool GetMouseButton(int button)
    {
        if (TryGetMouseButtonFromInputSystem(button, out bool isPressed))
        {
            return isPressed;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.GetMouseButton(button);
#else
        return false;
#endif
    }

    public static bool GetMouseButtonDown(int button)
    {
        if (TryGetMouseButtonDownFromInputSystem(button, out bool wasPressed))
        {
            return wasPressed;
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.GetMouseButtonDown(button);
#else
        return false;
#endif
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

    private static bool TryGetKeyFromInputSystem(KeyCode keyCode, out ButtonControl key)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            key = ResolveKey(keyboard, keyCode);
            return key != null;
        }
#endif

        key = null;
        return false;
    }

    private static bool TryGetAxisFromInputSystem(string axisName, out float value)
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        switch (axisName)
        {
            case "Horizontal":
                if (keyboard != null)
                {
                    value = GetButtonValue(keyboard, Key.D, Key.RightArrow) - GetButtonValue(keyboard, Key.A, Key.LeftArrow);
                    return true;
                }

                break;
            case "Vertical":
                if (keyboard != null)
                {
                    value = GetButtonValue(keyboard, Key.W, Key.UpArrow) - GetButtonValue(keyboard, Key.S, Key.DownArrow);
                    return true;
                }

                break;
            case "Mouse X":
                if (mouse != null)
                {
                    value = mouse.delta.ReadValue().x;
                    return true;
                }

                break;
            case "Mouse Y":
                if (mouse != null)
                {
                    value = mouse.delta.ReadValue().y;
                    return true;
                }

                break;
            case "Mouse ScrollWheel":
                if (mouse != null)
                {
                    value = mouse.scroll.ReadValue().y * 0.01f;
                    return true;
                }

                break;
        }
#endif

        value = 0f;
        return false;
    }

    public static Vector2 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            return mouse.position.ReadValue();
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return UnityEngine.Input.mousePosition;
#else
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
#endif
    }

    private static bool TryGetMouseButtonFromInputSystem(int button, out bool isPressed)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            switch (button)
            {
                case 0:
                    isPressed = mouse.leftButton.isPressed;
                    return true;
                case 1:
                    isPressed = mouse.rightButton.isPressed;
                    return true;
                case 2:
                    isPressed = mouse.middleButton.isPressed;
                    return true;
            }
        }
#endif

        isPressed = false;
        return false;
    }

    private static bool TryGetMouseButtonDownFromInputSystem(int button, out bool wasPressed)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            switch (button)
            {
                case 0:
                    wasPressed = mouse.leftButton.wasPressedThisFrame;
                    return true;
                case 1:
                    wasPressed = mouse.rightButton.wasPressedThisFrame;
                    return true;
                case 2:
                    wasPressed = mouse.middleButton.wasPressedThisFrame;
                    return true;
            }
        }
#endif

        wasPressed = false;
        return false;
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
