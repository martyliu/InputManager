using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager
{
    /// <summary>
    /// 切换正在使用的方案
    /// </summary>
    /// <param name="newScheme"></param>
    public static void ChangeUsingPCScheme(KeyboardMouseControlScheme newScheme)
    {
        Instance.ChangeUsingPCSchemeInternal(newScheme);
    }

    public static bool GetButton(string buttonName)
    {
        var keyboard = Instance.curKeyboardScheme;
        var joystick = Instance.curJoystickScheme;

        if (keyboard != null && joystick != null)
        {
            return keyboard.GetButton(buttonName)
                || joystick.GetButton(buttonName);
        }
        else if(keyboard != null)
        {
            return keyboard.GetButton(buttonName);
        }
        else  if(joystick != null)
        {
            return joystick.GetButton(buttonName);
        }

        return false;
    }


    public static bool GetButtonUp(string buttonName)
    {
        var keyboard = Instance.curKeyboardScheme;
        var joystick = Instance.curJoystickScheme;

        if (keyboard != null && joystick != null)
        {
            return keyboard.GetButtonUp(buttonName)
                || joystick.GetButtonUp(buttonName);
        }
        else if (keyboard != null)
        {
            return keyboard.GetButtonUp(buttonName);
        }
        else if (joystick != null)
        {
            return joystick.GetButtonUp(buttonName);
        }

        return false;
    }

    public static bool GetButtonDown(string buttonName)
    {
        var keyboard = Instance.curKeyboardScheme;
        var joystick = Instance.curJoystickScheme;

        if (keyboard != null && joystick != null)
        {
            return keyboard.GetButtonDown(buttonName)
                || joystick.GetButtonDown(buttonName);
        }
        else if (keyboard != null)
        {
            return keyboard.GetButtonDown(buttonName);
        }
        else if (joystick != null)
        {
            return joystick.GetButtonDown(buttonName);
        }

        return false;
    }


    public static float GetAxis(string axisName)
    {
        var keyboard = Instance.curKeyboardScheme;
        var joystick = Instance.curJoystickScheme;

        if (keyboard != null && joystick != null)
        {
            var result = keyboard.GetAxis(axisName);
            if (result == AXIS_ZERO)
                result = joystick.GetAxis(axisName);
            return result;
        }
        else if (keyboard != null)
        {
            return keyboard.GetAxis(axisName);
        }
        else if (joystick != null)
        {
            return joystick.GetAxis(axisName);
        }

        return AXIS_ZERO;
    }

    public static string[] GetJoystickNames()
    {
        if (_instance != null)
            return _instance.joysticksNameArray;
        return null;
    }
}
