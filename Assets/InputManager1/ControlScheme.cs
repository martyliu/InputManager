using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 配置方案
/// </summary>
[Serializable]
public class ControlScheme 
{
    [SerializeField]
    private string name;
    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    [SerializeField]
    private string description;
    public string Description
    {
        get { return description; }
        set { description = value; }
    }

    /// <summary>
    /// 是否自动匹配手柄
    /// </summary>
    [SerializeField]
    private bool isJoystickScheme;
    public bool IsJoystickScheme
    {
        get { return isJoystickScheme; }
        set { isJoystickScheme = value; }
    }

    [SerializeField]
    [Tooltip("匹配的手柄名称")]
    private string[] matchJoystickName;

    [SerializeField]
    private List<InputAction> actions;
    public List<InputAction> Actions => actions;

    /// <summary>
    /// Editor使用，是否展开
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private bool isExpanded = false;
    public bool IsExpaned
    {
        get { return isExpanded; }
        set { isExpanded = value; }
    }

    [NonSerialized]
    public int connectedJoystickIndex = -1;


    public void DoActive()
    {
        foreach (var a in actions)
            a.Initialize();
    }

    public void Update(float deltaTime)
    {
        foreach (var a in actions)
            a.Update(deltaTime);

    }

    public InputAction GetAction(string actionName)
    {
        return actions.Find(a => a.Name == actionName);
    }


    public bool GetButton(string buttonName)
    {
        var action = GetAction(buttonName);

        if (action == null)
        {
            Debug.LogError(buttonName + " action not exist ! ");
            return false;
        }
        return action.GetButton();
    }


    public bool GetButtonUp(string buttonName)
    {
        var action = GetAction(buttonName);

        if (action == null)
        {
            Debug.LogError(buttonName + " action not exist ! ");
            return false;
        }
        return action.GetButtonUp();
    }

    public bool GetButtonDown(string buttonName)
    {
        var action = GetAction(buttonName);

        if (action == null)
        {
            Debug.LogError(buttonName + " action not exist ! ");
            return false;
        }

        return action.GetButtonDown();
    }

    public float GetAxis(string axisName)
    {
        var action = GetAction(axisName);
        if(action == null)
        {
            Debug.LogError(axisName + " action not exist ! ");
            return 0.0f;

        }else
            return action.GetAxis();
    }

    public bool IsJoystickMatch(string joystickName)
    {
        if (!isJoystickScheme)
            return false;

        foreach(var m in matchJoystickName)
        {
            if (m.Contains(joystickName))
                return true;
        }
        return false;
    }

}
