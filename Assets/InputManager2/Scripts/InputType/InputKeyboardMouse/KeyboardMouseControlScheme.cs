using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class KeyboardMouseControlScheme : ControlSchemeBase
{
    public KeyboardMouseInputAction[] actions;

    public override InputActionBase[] m_actions => actions;

    public void Initialize()
    {
        foreach(var a in m_actions)
            a.Initialize();
    }
}
