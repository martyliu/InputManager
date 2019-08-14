using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class JoystickControlScheme : ControlSchemeBase
{
    [Tooltip("是否自动匹配手柄")]
    [SerializeField]
    private bool autoMatch;

    [Tooltip("匹配的手柄列表")]
    [SerializeField]
    private string[] matchJoysticks;

    public JoystickInputAction[] actions;

    public override InputActionBase[] m_actions => actions;

    public int connectedJoystickIdx { get; private set; } = -1;

    public void Initialize(int idx)
    {
        connectedJoystickIdx = idx;

        foreach (var a in m_actions)
            a.Initialize(connectedJoystickIdx);
    }

    public bool IsJoystickMatch(string joy)
    {
        if (!autoMatch)
            return false;

        foreach(var mj in matchJoysticks)
        {
            if (joy.Contains(mj))
                return true;
        }
        return false;
    }
}
