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

    [SerializeField]
    private string description;

    /// <summary>
    /// 是否自动匹配手柄
    /// </summary>
    [SerializeField]
    private bool isJoystickScheme;

    [SerializeField]
    [Tooltip("匹配的手柄名称")]
    private string[] matchJoystickName;

    [SerializeField]
    private List<InputAction> actions;

    /// <summary>
    /// Editor使用，是否展开
    /// </summary>
    [HideInInspector]
    [SerializeField]
    private bool isExpanded = false;

    public List<InputAction> Actions => actions;
    public bool IsExpaned
    {
        get { return isExpanded; }
        set { isExpanded = value; }
    }
    

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public string Description
    {
        get { return description; }
        set { description = value; }
    }



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
}
