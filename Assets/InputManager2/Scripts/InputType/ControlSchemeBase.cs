﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; 

[Serializable]
public abstract class ControlSchemeBase
{
    public abstract InputActionBase[] m_actions { get; }

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


    public virtual void Update(float dt)
    {
        foreach (var a in m_actions)
            a.Update(dt);
    }

    public InputActionBase GetAction(string actionName)
    {
        return Array.Find(m_actions, a => a.Name == actionName);
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
        if (action == null)
        {
            Debug.LogError(axisName + " action not exist ! ");
            return 0.0f;

        }
        else
            return action.GetAxis();
    }

}
