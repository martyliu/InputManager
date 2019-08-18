using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可在游戏中更改的值
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class ModifiableValue<T> where T : struct
{
    Action onValueChange;


    [SerializeField]
    private T m_defaultVal;

    private T? m_customVal;

    public T value
    {
        get
        {
            if (m_customVal.HasValue)
                return m_customVal.Value;
            else
                return m_defaultVal;
        }
    }

    public bool HasCustom { get { return m_customVal.HasValue; } }

    public void SetCustom(T newVal)
    {
        m_customVal = newVal;

        onValueChange?.Invoke();
    }

    public void ChangeDefault(T d)
    {
        m_defaultVal = d;

        onValueChange?.Invoke();
    }

    public void ResetToDefault()
    {
        m_customVal = null;
    }

    /// <summary>
    /// 监听值的变化
    /// </summary>
    /// <param name="valueChange"></param>
    public void RegisterValueChange(Action valueChange)
    {
        onValueChange -= valueChange;
        onValueChange += valueChange;
    }

    public void UnRegisterValueChange(Action valueChange)
    {
        onValueChange -= valueChange;
    }

    public ModifiableValue<T> Copy()
    {
        var b = new ModifiableValue<T>();
        b.m_defaultVal = this.m_defaultVal;
        b.m_customVal = this.m_customVal;
        return b;
    }
}

[Serializable]
public class BoolModifiable : ModifiableValue<bool> { }

[Serializable]
public class IntModifiable : ModifiableValue<int> { }

[Serializable]
public class JoystickButtonModifiable : ModifiableValue<JoystickButton> { }

[Serializable]
public class KeyCodeModifiable : ModifiableValue<KeyCode> { }

