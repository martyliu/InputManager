﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class InputActionBase
{
    protected abstract IInputBinding[] m_bindings { get; }

    [SerializeField]
    private string m_name;

    [SerializeField]
    private string description;

    public string Name
    {
        get { return m_name; }
        set { m_name = value; }
    }

    public void Initialize(int joystickIdx = -1)
    {
        foreach (var b in m_bindings)
            b.Initialize(joystickIdx);
    }

    public void Update(float dt)
    {
        foreach (var b in m_bindings)
            b.Update(dt);
    }

    public bool GetButton()
    {
        foreach (var b in m_bindings)
        {
            var val = b.GetButton();
            if (val.HasValue)
                return val.Value;
        }
        return false;
    }

    public bool GetButtonUp()
    {
        foreach (var b in m_bindings)
        {
            var val = b.GetButtonUp();
            if (val.HasValue)
                return val.Value;
        }
        return false;
    }

    public bool GetButtonDown()
    {
        foreach (var b in m_bindings)
        {
            var val = b.GetButtonDown();
            if (val.HasValue)
                return val.Value;
        }
        return false;
    }

    public float GetAxis()
    {
        foreach (var b in m_bindings)
        {
            var val = b.GetAxis();
            if (val.HasValue)
                return val.Value;
        }
        return InputManager.AXIS_ZERO;
    }
}