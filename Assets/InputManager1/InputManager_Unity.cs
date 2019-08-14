using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager
{
    /// <summary>
    /// 切换正在使用的方案
    /// </summary>
    /// <param name="newScheme"></param>
    public static void ChangeUsingScheme(ControlScheme newScheme)
    {
        Instance.ChangeUsingSchemeInternal(newScheme);
    }

    public static void ChangeUsingScheme(int index)
    {
        if (index >= 0 && index < Instance.Schemes.Count)
            Instance.ChangeUsingSchemeInternal(Instance.Schemes[index]);
    }

    public static bool GetButton(string buttonName, int schemeIndex = -1)
    {
        if(Instance.curUsingScheme != null)
        {
            return Instance.curUsingScheme.GetButton(buttonName);
        }
        return false;
    }


    public static bool GetButtonUp(string buttonName)
    {
        if(Instance.curUsingScheme != null)
            return Instance.curUsingScheme.GetButtonUp(buttonName);
        return false;
    }

    public static bool GetButtonDown(string buttonName)
    {

        if(Instance.curUsingScheme != null)
            return Instance.curUsingScheme.GetButtonDown(buttonName);
        return false;
    }


    public static float GetAxis(string axisName)
    {
        if(Instance.curUsingScheme != null)
            return Instance.curUsingScheme.GetAxis(axisName);
        return 0.0f;
    }

    public static string[] GetJoystickNames()
    {
        if (_instance != null)
            return _instance.joysticksNameArray;
        return null;
    }
}
